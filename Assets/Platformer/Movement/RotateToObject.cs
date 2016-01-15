using UnityEngine;
using System.Collections;

namespace Sjabloon
{
    public class RotateToObject : MoveableObject
    {
        [SerializeField]
        private GameObject m_FollowingObject;

        [SerializeField]
        private float m_RotationSpeed;

        public override void Move()
        {
            Vector3 dir = m_FollowingObject.transform.position - transform.position;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetQuaternion = Quaternion.AngleAxis(angle - 90.0f, Vector3.forward);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetQuaternion, m_RotationSpeed * Time.deltaTime);
        }
    }
}