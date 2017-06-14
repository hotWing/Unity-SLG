using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace UI
{
    public class TurnIdicator : MonoBehaviour
    {
        public static TurnIdicator instance;
        public static event EventHandler OnEnd;
        public AnimationCurve animationCurve;
        private Transform textTrans;
        private RectTransform textRectTrans;
        void Start()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            instance = this;

            textTrans = transform.GetChild(0);
            gameObject.SetActive(false);
            textRectTrans = textTrans.GetComponent<RectTransform>();
        }

        public void showTurn(string msg, Color color)
        {
            Text turnText = textTrans.GetComponent<Text>();
            turnText.color = color;
            turnText.text = msg;
            gameObject.SetActive(true);
            Tweener tw = textRectTrans.DOMoveX(Screen.width, 2).SetEase(animationCurve);
            tw.OnComplete(()=>{
                textRectTrans.anchoredPosition = Vector2.zero;
                gameObject.SetActive(false);
                if (OnEnd != null)
                    OnEnd(this, null);
            });
        }
    }
}
