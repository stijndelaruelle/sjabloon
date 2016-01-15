using UnityEngine;
using System.Collections;

namespace Platformer
{
    [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
    public class CharacterController2D : MonoBehaviour
    {
        [SerializeField]
        private BoxCollider2D m_BoxCollider;
        private Bounds m_BoundsWithSkin;

        [SerializeField]
        private LayerMask m_PlatformMask = 0;
        public LayerMask PlatformMask
        {
            get { return m_PlatformMask; }
            set { m_PlatformMask = value; }
        }

        [SerializeField]
        [Range(2, 20)]
        private int m_TotalHorizontalRays = 8;
        private float m_DistanceBetweenHorizontalRays = 0.0f;

        [SerializeField]
        [Range(2, 20)]
        private int m_TotalVerticalRays = 4;
        private float m_DistanceBetweenVerticalRays = 0.0f;

        [SerializeField]
        [Range(0, 90f)]
        private float m_SlopeLimit = 30f;

        [SerializeField]
        [Range(0.001f, 0.3f)]
        private float m_SkinWidth = 0.02f;

        private bool m_IsGrounded;
        public bool IsGrounded
        {
            get { return m_IsGrounded; }
        }

        private bool m_IsGoingUpSlope;

        private void Start()
        {
            //Horizontal
            float colliderUseableWidth = m_BoxCollider.bounds.size.y * Mathf.Abs(transform.localScale.y) - (2.0f * m_SkinWidth);
            m_DistanceBetweenHorizontalRays = colliderUseableWidth / (m_TotalHorizontalRays - 1);

            //Vertical
            float colliderUseableHeight = m_BoxCollider.bounds.size.x * Mathf.Abs(transform.localScale.x) - (2.0f * m_SkinWidth);
            m_DistanceBetweenVerticalRays = colliderUseableHeight / (m_TotalVerticalRays - 1);
        }

        public Vector2 Move(float deltaX, float deltaY)
        {
            //Update bounds
            m_BoundsWithSkin = m_BoxCollider.bounds;
            m_BoundsWithSkin.Expand(-2.0f * m_SkinWidth);

            m_IsGrounded = false;
            m_IsGoingUpSlope = false;

            Vector2 deltaMovement = new Vector2(deltaX * Time.deltaTime, deltaY * Time.deltaTime);

            //Move horizontally
            HandleHorizontalMovement(ref deltaMovement);
            
            //Move vertically
            HandleVerticalMovement(ref deltaMovement);

            //Do the actual movement in world space
            transform.Translate(new Vector3(deltaMovement.x, deltaMovement.y, 0.0f), Space.World);

            //Return the current velocity
            Vector2 velocity = new Vector2(0.0f, 0.0f);

            if (Time.deltaTime > 0.0f)
                velocity = deltaMovement / Time.deltaTime;

            return velocity;
        }

        private void HandleHorizontalMovement(ref Vector2 deltaMovement)
        {
            //Check horizontal movement
            if (deltaMovement.x != 0)
            {
                bool isGoingRight = (deltaMovement.x > 0);
                float rayDistance = Mathf.Abs(deltaMovement.x) + m_SkinWidth; //We cast a ray as long as we're willing to move

                Vector2 rayDirection = new Vector2();
                Vector3 rayPosition = new Vector3();

                for (int i = 0; i < m_TotalHorizontalRays; i++)
                { 
                    if (isGoingRight)
                    {
                        rayDirection.x = 1.0f;
                        rayPosition.x = m_BoundsWithSkin.max.x;
                    }
                    else
                    {
                        rayDirection.x = -1.0f;
                        rayPosition.x = m_BoundsWithSkin.min.x;
                    }

                    rayPosition.y = m_BoundsWithSkin.min.y + (i * m_DistanceBetweenHorizontalRays);
                    rayPosition.z = 0.0f;

                    Debug.DrawRay(rayPosition, rayDirection * rayDistance, Color.red);

                    RaycastHit2D raycastHit = Physics2D.Raycast(rayPosition, rayDirection, rayDistance, m_PlatformMask);

                    if (raycastHit)
                    {
                        if (i == 0)
                        {
                            HandleUpwardSlope(ref deltaMovement, Vector2.Angle(raycastHit.normal, Vector2.up));
                            return;
                        }

                        float tempDeltaX = raycastHit.point.x - rayPosition.x;

                        if (isGoingRight) { tempDeltaX -= m_SkinWidth; }
                        else              { tempDeltaX += m_SkinWidth; }

                        if (Mathf.Abs(tempDeltaX) < Mathf.Abs(deltaMovement.x))
                            deltaMovement.x = tempDeltaX;
                    }
                }
            }
        }

        private void HandleUpwardSlope(ref Vector2 deltaMovement, float angle)
        {
            if (angle >= m_SlopeLimit)
            {
                deltaMovement.x = 0.0f;
                return;
            }

            // we only need to adjust the deltaMovement if we are not jumping
            // TODO: this uses a magic number which isn't ideal!
            if (deltaMovement.y < 0.002f)
            {
                deltaMovement.y = Mathf.Abs(Mathf.Tan(angle * Mathf.Deg2Rad) * deltaMovement.x);
                m_IsGoingUpSlope = true;
                m_IsGrounded = true;
            }
        }

        private void HandleVerticalMovement(ref Vector2 deltaMovement)
        {
            //Check vertical movement
            if (deltaMovement.y != 0)
            {
                bool firstRaycastHit = true;

                bool isGoingUp = (deltaMovement.y > 0) && (m_IsGoingUpSlope == false);
                float rayDistance = Mathf.Abs(deltaMovement.y) + m_SkinWidth; //We cast a ray as long as we're willing to move

                Vector2 rayDirection = new Vector2();
                Vector3 rayPosition = new Vector3();

                for (int i = 0; i < m_TotalVerticalRays; i++)
                { 
                    if (isGoingUp)
                    {
                        rayDirection.y = 1.0f;
                        rayPosition.y = m_BoundsWithSkin.max.y;  
                    }
                    else
                    {
                        rayDirection.y = -1.0f;
                        rayPosition.y = m_BoundsWithSkin.min.y;
                    }

                    rayPosition.x = m_BoundsWithSkin.min.x + (i * m_DistanceBetweenVerticalRays);
                    rayPosition.z = 0.0f;

                    Debug.DrawRay(rayPosition, rayDirection * rayDistance, Color.yellow);

                    RaycastHit2D raycastHit = Physics2D.Raycast(rayPosition, rayDirection, rayDistance, m_PlatformMask);

                    if (raycastHit)
                    {
                        if (!m_IsGoingUpSlope)
                        {
                            float tempDeltaY = raycastHit.point.y - rayPosition.y;

                            if (isGoingUp) { tempDeltaY -= m_SkinWidth; }
                            else           { tempDeltaY += m_SkinWidth; }

                            if ((Mathf.Abs(tempDeltaY) < Mathf.Abs(deltaMovement.y)) || firstRaycastHit)
                                deltaMovement.y = tempDeltaY;
                        }

                        firstRaycastHit = false;
                        m_IsGrounded = true;
                    }
                }
            }
        }
    }
}