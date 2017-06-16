using UnityEngine;
using DG.Tweening;
using Character;
using System;
using System.Collections;

namespace UI
{
    public class AttackUI : MonoBehaviour
    {
        [HideInInspector]
        public static AttackUI instance;
        public static bool showing;
        private Action<AttackType> OnAttackTypeSet;
        private RectTransform handler;
        private Tweener handlerTw;
        // Use this for initialization
        void Start()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            instance = this;
            handler = transform.GetChild(1).GetComponent<RectTransform>();
            gameObject.SetActive(false);
            showing = false;
        }

        private void OnClick(Vector2 clickPos)
        {
            handlerTw.Pause();
            StartCoroutine(getAttackType());
        }

        private IEnumerator getAttackType()
        {
            yield return new WaitForSeconds(0.5f);

            float x = handler.anchoredPosition.x;
            if (OnAttackTypeSet != null)
            {
                if (x <= 110 || x >= 190)
                    OnAttackTypeSet(AttackType.Normal);
                else if (x < 140 || x > 160)
                    OnAttackTypeSet(AttackType.Miss);
                else
                    OnAttackTypeSet(AttackType.Critical);
            }
            handler.anchoredPosition = Vector2.zero;
            Hide();
        }

        public void Show(Action<AttackType> OnAttackTypeSet)
        {
            showing = true;
            gameObject.SetActive(true);
            handlerTw = handler.DOLocalMoveX(300, 2);
            handlerTw.SetEase(Ease.InOutQuart);
            handlerTw.SetLoops(-1, LoopType.Yoyo);
            this.OnAttackTypeSet = OnAttackTypeSet;
        }

        public void Hide()
        {
            showing = false;
            gameObject.SetActive(false);
        }

        void OnEnable()
        {
            GameInput.OnClick += OnClick;
        }

        void OnDisable()
        {
            GameInput.OnClick -= OnClick;
            OnAttackTypeSet = null;
            handlerTw.Kill();
        }
    }
}
