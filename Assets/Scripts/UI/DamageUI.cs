using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace UI
{
    public class DamageUI : MonoBehaviour
    {
        [HideInInspector]
        public static DamageUI instance;
        private Text dmgText;
        // Use this for initialization
        void Start()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            instance = this;
            dmgText = transform.GetComponentInChildren<Text>();
            gameObject.SetActive(false);
        }


        public void Show(Vector2 pos, string dmg)
        {
            transform.position = pos;
            dmgText.text = dmg;
            gameObject.SetActive(true);
            Sequence mySequence = DOTween.Sequence();
            mySequence.Append(transform.DOMove(pos + new Vector2(5, 5), 1));
            mySequence.Join(transform.DOScale(1.5f, 1));
            mySequence.OnComplete(() => { gameObject.SetActive(false);});
        }
    }
    
}
