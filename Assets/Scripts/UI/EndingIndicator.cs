using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EndingIndicator : MonoBehaviour
    {

        public static EndingIndicator instance;
        private Text info;
        // Use this for initialization
        void Start()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            instance = this;

            info = transform.GetComponentInChildren<Text>();
            gameObject.SetActive(false);
        }

        // Update is called once per frame
        public void Show(string text)
        {
            gameObject.SetActive(true);
            info.text = text;
        }
    }
}
