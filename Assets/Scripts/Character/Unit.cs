using GridSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(Rigidbody), typeof(Animator))]
    public class Unit : MonoBehaviour
    {
        public string name = "";
        public int speed = 2;
        public int attackRange = 1;
        public int attackPower = 10;
        public int defence = 5;
        public int hp = 20;

        private Team team;
        public Team Team
        {
            get { return team; }
            set { team = value; }
        }
        private UnitStatus status;
        public UnitStatus Status
        {
            get { return status; }
        }

        private Renderer renderer1;
        private Rigidbody rigBody;
        private Animator animator;

        private HealthBar healthBar;
        private Unit attackee;

        public static Action<Unit> OnUnitIdle;
        public static Action OnMoveComplete;
        public static Action<Unit> OnDead;

        private Vector3 returnPos;
        private Quaternion returnRot;

        bool canIdle; //攻击动作完成后，需不需要等待被攻击unit死亡动画再idle
        void Start()
        {

            rigBody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            renderer1 = GetComponentInChildren<Renderer>();
            GameObject nodeObj = Grid.instance.getNodeObjFromPosition(transform.position);
            Grid.instance.setNodeStatus(nodeObj, NodeStatus.Occupied);
            setStatus(UnitStatus.Ready);
            initHealthBar();
        }

        void Update()
        {
            Vector3 healBarPos = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position);
            healthBar.transform.position = healBarPos;
        }

        private void initHealthBar()
        {
            GameObject healthBarObj = Instantiate(GameManager.healthBarPrefab);
            healthBar = healthBarObj.GetComponent<HealthBar>();
            healthBar.setHpMax(hp);
            healthBar.setHp(hp);
        }

        public void cancelMove()
        {
            GameObject nodeObj = Grid.instance.getNodeObjFromPosition(transform.position);
            Grid.instance.setNodeStatus(nodeObj, NodeStatus.Normal);
            transform.position = returnPos;
            transform.rotation = returnRot;
            nodeObj = Grid.instance.getNodeObjFromPosition(transform.position);
            Grid.instance.setNodeStatus(nodeObj, NodeStatus.Occupied);
            setStatus(UnitStatus.Ready);
        }


        public void move(List<GameObject> path)
        {
            StartCoroutine(moveCoroutine(path));
        }

        private IEnumerator moveCoroutine(List<GameObject> path)
        {
            returnPos = transform.position;
            returnRot = transform.rotation;

            status = UnitStatus.Moving;
            playRunAnim();
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 startPos = path[i].transform.position;
                Vector3 endPos = path[i + 1].transform.position;

                GameObject nodeObj = Grid.instance.getNodeObjFromPosition(startPos);
                Grid.instance.setNodeStatus(nodeObj, NodeStatus.Normal);

                Vector3 newDirection = (endPos - startPos).normalized;
                newDirection.y = 0;
                transform.forward = newDirection;

                float dist = Vector3.Distance(startPos, endPos);
                float distTraveled = 0;
                endPos.y = startPos.y = rigBody.position.y;
                while (!rigBody.position.Equals(endPos))
                {
                    distTraveled += GameManager.unitMoveSpeed * Time.deltaTime;
                    rigBody.position = Vector3.Lerp(startPos, endPos, distTraveled / dist);
                    yield return null;
                }
                nodeObj = Grid.instance.getNodeObjFromPosition(endPos);
                Grid.instance.setNodeStatus(nodeObj, NodeStatus.Occupied);
            }
            playIdleAnim();
            status = UnitStatus.Moved;
            OnMoveComplete();
        }


        public void attack(Unit attackee)
        {
            status = UnitStatus.Attacking;

            Vector3 faceDir = (attackee.transform.position - transform.position).normalized;
            faceDir.y = 0;
            transform.forward = faceDir;

            animator.SetTrigger("attack");
            this.attackee = attackee;
        }

        private void playRunAnim()
        {
            animator.SetBool("run", true);
        }

        private void playIdleAnim()
        {
            animator.SetBool("run", false);
        }

        //private IEnumerator playAttackAnim(Unit attackee, Action callback)
        //{
        //    animator.SetTrigger("attack");
        //    yield return null;
        //    while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1)
        //    {
        //        yield return null;
        //    }
        //    attackee.takeDamage(attackPower - attackee.defence);
        //    callback();
        //}

        public void setStatus(UnitStatus newStatus)
        {
            switch (newStatus)
            {
                case UnitStatus.Idle:
                    status = UnitStatus.Idle;
                    renderer1.material.color = Color.red;
                    OnUnitIdle(this);
                    break;
                case UnitStatus.Ready:
                    status = UnitStatus.Ready;
                    renderer1.material.color = Color.white;
                    break;
            }
        }

        public void takeDamage(Unit attacker,int damage)
        {
            int curHp = healthBar.getHp() - damage;
            if (curHp <= 0)
            {
                healthBar.setHp(0);
                attacker.canIdle = false;
                animator.SetTrigger("die");                
            }
            else
            {
                healthBar.setHp(healthBar.getHp() - damage);
                attacker.canIdle = true;
            }
        }

        public void attackAminComplete()
        {
            if(canIdle)
                setStatus(UnitStatus.Idle);
        }

        public void attackHit()
        {
            attackee.takeDamage(this, attackPower - attackee.defence);
        }

        public void die()
        {
            Destroy(gameObject);
            Destroy(healthBar.gameObject);
            OnDead(this);
        }
    }

    public enum UnitStatus
    {
        Idle,
        Ready,
        Moving,
        Moved,
        Attacking,
        Null
    }

}
