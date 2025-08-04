using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script
{
    public class DamagePopupPool : MonoBehaviour
    {
        public static DamagePopupPool Instance;
        [SerializeField] GameObject popupPrefab;
        private Queue<GameObject> pool = new Queue<GameObject>();

        void Awake()
        {
            Instance = this;
        }

        public GameObject GetPopup()
        {
            if (pool.Count > 0)
            {
                var go = pool.Dequeue();
                go.SetActive(true);
                return go;
            }
            return Instantiate(popupPrefab);
        }

        public void ReturnPopup(GameObject popup)
        {
            popup.SetActive(false);
            pool.Enqueue(popup);
        }
    }

}
