﻿using UnityEngine;
using System;
using UI;

public class GameInput:MonoBehaviour{

    public static event Action<Vector2> OnClick;
	void Update () {
        if(!AIController.isEnermyMoving)
        {
            if (Input.GetMouseButtonUp(0))
            {
                //没有点击UI，防止点击移动后，马上就移动，因为点击移动的同时，raycast hit到了ui后面的node
                //if (!EventSystem.current.IsPointerOverGameObject())
                //{
                if (!BattleMenu.isMouseover)
                {
                    OnClick(Input.mousePosition);
                }
                //}
            }
        }
    }
}
