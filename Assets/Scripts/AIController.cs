using UnityEngine;
using System.Collections.Generic;
using Character;
using GridSystem;
using System;

public class AIController {
    public static bool isEnermyMoving = false;

    public static List<Unit> enermyTeam;
    public static List<Unit> playerTeam;

    private static int enermyIndex;
    private static Unit attackee;
    public static void OnTurnIndicatorEnd(object sender, EventArgs e)
    {
        isEnermyMoving = true;

        enermyIndex = 0;
        nextEnermyUnit();
    }

    private static void nextEnermyUnit()
    {
        Unit enermyUnit = enermyTeam[enermyIndex];
        List<GameObject> path = getPathToNearestPlayerUnit();
        enermyUnit.move(path);
    }

    private static List<GameObject> getPathToNearestPlayerUnit()
    {
        Unit enermyUnit = enermyTeam[enermyIndex];
        List<GameObject> minPath = null;
        foreach (Unit playerUnit in playerTeam)
        {
            GameObject startNodeObj = Grid.instance.getNodeObjFromPosition(enermyUnit.transform.position);
            GameObject endNodeObj = Grid.instance.getNodeObjFromPosition(playerUnit.transform.position);
            List<GameObject> curPath =  Grid.instance.getPathToEnermy(startNodeObj, endNodeObj,enermyUnit.attackRange);
            if (minPath == null || curPath.Count < minPath.Count)
            {
                attackee = playerUnit;
                minPath = curPath;
            }
        }

        //没有到达可攻击的位置，去掉path中超出移动力的nodes
        if (minPath.Count - 1 > enermyUnit.speed)
        {
            minPath.RemoveRange(enermyUnit.speed + 1, minPath.Count - 1 - enermyUnit.speed);
            attackee = null;//设为null来指示没到达可以攻击位置，不能攻击
        }

        return minPath;
    }

    public static void onUnitAttackComplete()
    {
        //Unit enermyUnit = enermyTeam[enermyIndex];
        //enermyUnit.setStatus(UnitStatus.Idle);
    }
    public static void onUnitIdle(Unit enermyUnit)
    {
        enermyIndex++;
        if (enermyIndex < enermyTeam.Count)
            nextEnermyUnit();
        else
        {
            isEnermyMoving = false;
            GameManager.instance.switchTeam();
        }
    }
    public static void onUnitMoveComplete()
    {
        Unit enermyUnit = enermyTeam[enermyIndex];
        if (attackee != null)
            enermyUnit.attack(attackee);
        else
            enermyUnit.setStatus(UnitStatus.Idle);
    }
}
