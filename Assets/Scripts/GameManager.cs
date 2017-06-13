using UnityEngine;
using GridSystem;
using System.Collections.Generic;
using Character;
using UI;
using System;

public enum Team
{
    Team1,
    Team2
}

public class GameManager : MonoBehaviour
{
    //public static int refWidth = 800;
    public static float unitMoveSpeed = 5f;
    public static GameManager instance;
    public static Unit selectedUnit;
    public static GameObject selectedUnitNodeObj;
    public static GameObject healthBarContainerObj;
    public static GameObject healthBarPrefab;

    private Team currentTurnTeam;//当前行动的阵营
    private List<Unit> currentNotPlayedUnits;
    public static List<Unit> team1Units;
    public static List<Unit> team2Units;
    private int turnCount;

    public static int nodeLayer;
    public static int unitLayer;
    private GameObject nodeObjMouseOver;
    void Start()
    {
        // if the singleton hasn't been initialized yet
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        GameInput.OnClick += OnClick;

        initUnits();

        healthBarContainerObj = GameObject.FindGameObjectWithTag("HealthBarContainer") as GameObject;
        healthBarPrefab = Resources.Load("Prefabs/UI/HealthBar") as GameObject;

        currentTurnTeam = Team.Team1;
        currentNotPlayedUnits = new List<Unit>(team1Units);
        turnCount = 1;
        TurnIdicator.instance.showTurn("Turn " + turnCount, Color.blue);

        nodeLayer = LayerMask.NameToLayer("Node");
        unitLayer = LayerMask.NameToLayer("Unit");

    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, 1 << nodeLayer))
        {
            GameObject hitNodeObj = Grid.instance.getNodeObjFromPosition(hit.point);
            if (!hitNodeObj.Equals(nodeObjMouseOver))
            {
                if (nodeObjMouseOver != null)
                    nodeObjMouseOver.GetComponent<Renderer>().material.SetInt("_Animate", 0);
                nodeObjMouseOver = hitNodeObj;
                nodeObjMouseOver.GetComponent<Renderer>().material.SetInt("_Animate", 1);
            }
        }
        else
        {
            if (nodeObjMouseOver != null)
            {
                nodeObjMouseOver.GetComponent<Renderer>().material.SetInt("_Animate", 0);
                nodeObjMouseOver = null;
            }
        }

    }

    /// <summary>
    ///创建角色
    /// </summary>
    private void initUnits()
    {
        GameObject swordManPrefab = Resources.Load("Prefabs/Units/SwordMan") as GameObject;
        GameObject spearManPrefab = Resources.Load("Prefabs/Units/SpearMan") as GameObject;
        GameObject archerPrefab = Resources.Load("Prefabs/Units/Archer") as GameObject;

        Quaternion team1Rot = Quaternion.Euler(0, 90, 0);
        team1Units = new List<Unit>();
        addUnit(swordManPrefab, Team.Team1, Grid.instance.NodeObjs[0, 3].transform.position, team1Rot, team1Units);
        addUnit(spearManPrefab, Team.Team1, Grid.instance.NodeObjs[0, 4].transform.position, team1Rot, team1Units);
        addUnit(archerPrefab, Team.Team1, Grid.instance.NodeObjs[0, 5].transform.position, team1Rot, team1Units);

        Quaternion team2Rot = Quaternion.Euler(0, 270, 0);
        Texture team2Tex = Resources.Load("Textures/WK_StandardUnits_Red") as Texture;
        team2Units = new List<Unit>();
        addUnit(swordManPrefab, Team.Team2, Grid.instance.NodeObjs[9, 3].transform.position, team2Rot, team2Units, team2Tex);
        addUnit(spearManPrefab, Team.Team2, Grid.instance.NodeObjs[9, 4].transform.position, team2Rot, team2Units, team2Tex);
        addUnit(archerPrefab, Team.Team2, Grid.instance.NodeObjs[9, 5].transform.position, team2Rot, team2Units, team2Tex);

        Grid.instance.setNodeStatus(Grid.instance.NodeObjs[0, 3], NodeStatus.Occupied);
        Grid.instance.setNodeStatus(Grid.instance.NodeObjs[0, 4], NodeStatus.Occupied);
        Grid.instance.setNodeStatus(Grid.instance.NodeObjs[0, 5], NodeStatus.Occupied);
        Grid.instance.setNodeStatus(Grid.instance.NodeObjs[9, 3], NodeStatus.Occupied);
        Grid.instance.setNodeStatus(Grid.instance.NodeObjs[9, 4], NodeStatus.Occupied);
        Grid.instance.setNodeStatus(Grid.instance.NodeObjs[9, 5], NodeStatus.Occupied);


        Unit.OnMoveComplete = onUnitMoveComplete;
        Unit.OnUnitIdle = onUnitIdle;
        Unit.OnDead = onUnitDead;
    }

    private void onUnitDead(Unit unit)
    {
        GameObject nodeObj = Grid.instance.getNodeObjFromPosition(unit.transform.position);
        nodeObj.GetComponent<Node>().changeStatus(NodeStatus.Normal);
        team2Units.Remove(unit);
        if (team2Units.Count == 0)
            EndingIndicator.instance.Show("胜利！！");
        else
            selectedUnit.setStatus(UnitStatus.Idle);
    }

    private void addUnit(GameObject unitPrefab, Team team, Vector3 pos, Quaternion rot, List<Unit> teamUnits, Texture texture = null)
    {
        GameObject unitObj = Instantiate(unitPrefab, pos, rot);
        Unit unit = unitObj.GetComponent<Unit>();
        unit.Team = team;
        if (texture != null)
            unitObj.GetComponentInChildren<Renderer>().material.mainTexture = texture;
        teamUnits.Add(unit);
    }

    void OnClick(Vector2 clickPos)
    {
        UnitStatus selectedUnitStatus = UnitStatus.Null;
        if (selectedUnit != null)
            selectedUnitStatus = selectedUnit.Status;

        if (selectedUnitStatus != UnitStatus.Moving
            && selectedUnitStatus != UnitStatus.Attacking
            && selectedUnitStatus != UnitStatus.Moved)
        {
            Node hitNode = getHitObject<Node>(clickPos, 1 << nodeLayer);
            if (hitNode != null)
            {
                if (hitNode.Status == NodeStatus.Occupied)
                {
                    Unit hitUnit = Grid.instance.getUnitOnNode(hitNode);
                    if (hitUnit != null && hitUnit.Status == UnitStatus.Ready && hitUnit.Team == currentTurnTeam)
                    {
                        setSelectedUnit(hitUnit);
                    }
                }
                else if (hitNode.Status == NodeStatus.Attackable)
                {
                    //Vector3 faceDir = (hitNode.transform.position - selectedUnit.transform.position).normalized;
                    //faceDir.y = 0;
                    //selectedUnit.transform.forward = faceDir;
                    selectedUnit.attack(Grid.instance.getUnitOnNode(hitNode));
                    Grid.instance.clear();
                    BattleMenu.instance.hideIdle();
                }
                else if (hitNode.Status == NodeStatus.Movable)
                {
                    List<GameObject> path = Grid.instance.getShortestPath(selectedUnitNodeObj, hitNode.gameObject);
                    selectedUnit.move(path);
                    Grid.instance.clear();
                    BattleMenu.instance.hideIdle();
                }
            }
        }
        else if (selectedUnitStatus == UnitStatus.Moved)
        {
            Node hitNode = getHitObject<Node>(clickPos, 1 << nodeLayer);
            if (hitNode != null)
            {
                if (hitNode.Status == NodeStatus.Attackable)
                {
                    BattleMenu.instance.hide();

                    Vector3 faceDir = (hitNode.transform.position - selectedUnit.transform.position).normalized;
                    faceDir.y = 0;
                    selectedUnit.transform.forward = faceDir;
                    Grid.instance.clear();
                    selectedUnit.attack(Grid.instance.getUnitOnNode(hitNode));
                }
            }
        }
    }

    private T getHitObject<T>(Vector2 clickPos, LayerMask layermask)
    {
        Ray ray = Camera.main.ScreenPointToRay(clickPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, layermask))
        {
            GameObject hitGo = hit.collider.gameObject;
            return hitGo.GetComponent<T>();
        }
        return default(T);
    }

    private void onUnitMoveComplete()
    {
        Grid.instance.highLightUnitAttackable();
        BattleMenu.instance.show();
    }

    private void setSelectedUnit(Unit unit)
    {
        Grid.instance.clear();
        selectedUnit = unit;
        selectedUnitNodeObj = Grid.instance.getNodeObjFromPosition(unit.transform.position);
        Grid.instance.hightLightUnitMovable();
        Grid.instance.highLightUnitAttackable();

        BattleMenu.instance.showIdle();
    }

    private void onUnitIdle(Unit unitOnIdle)
    {
        currentNotPlayedUnits.Remove(unitOnIdle);
        Grid.instance.clear();
        if (currentNotPlayedUnits.Count == 0)
            switchTeam();
    }

    public void switchTeam()
    {
        selectedUnit = null;
        switch (currentTurnTeam)
        {
            case Team.Team1://切换到敌人

                resetTeamStatus(team1Units);
                currentTurnTeam = Team.Team2;
                currentNotPlayedUnits = new List<Unit>(team2Units);
                TurnIdicator.OnEnd += AIController.OnTurnIndicatorEnd;
                TurnIdicator.instance.showTurn("Enermy's Turn", Color.red);
                Unit.OnUnitIdle = AIController.onUnitIdle;
                Unit.OnMoveComplete = AIController.onUnitMoveComplete;
                Unit.OnDead = AIController.onUnitDead;
                break;
            case Team.Team2:
                resetTeamStatus(team2Units);
                currentTurnTeam = Team.Team1;
                currentNotPlayedUnits = new List<Unit>(team1Units);
                TurnIdicator.OnEnd -= AIController.OnTurnIndicatorEnd;
                TurnIdicator.instance.showTurn("Turn " + ++turnCount, Color.blue);
                Unit.OnUnitIdle = onUnitIdle;
                Unit.OnMoveComplete = onUnitMoveComplete;
                Unit.OnDead = onUnitDead;
                break;
        }
    }

    private void resetTeamStatus(List<Unit> teamUnits)
    {
        foreach (Unit unit in teamUnits)
        {
            unit.setStatus(UnitStatus.Ready);
        }
    }

}


