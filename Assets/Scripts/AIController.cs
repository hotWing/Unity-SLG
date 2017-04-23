using UnityEngine;
using System.Collections.Generic;
using Character;
using GridSystem;

public class AIController {
    public static bool isEnermyMoving = false;

    public static List<Unit> enermyTeam;
    public static List<Unit> playerTeam;

    private static int enermyIndex;

    public static void takeAction()
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
                minPath = curPath;
        }

        //去掉path中超出移动力的nodes
        if (minPath.Count - 1 > enermyUnit.speed)
            minPath.RemoveRange(enermyUnit.speed+1, minPath.Count-1 - enermyUnit.speed);

        return minPath;
    }

    public static void onUnitAttackComplete()
    {

    }
    public static void onUnitIdle(Unit enermyUnit)
    {
        enermyIndex++;
        nextEnermyUnit();
    }
    public static void onUnitMoveComplete()
    {
        Unit enermyUnit = enermyTeam[enermyIndex];
        enermyUnit.setStatus(UnitStatus.Idle);
    }
}
