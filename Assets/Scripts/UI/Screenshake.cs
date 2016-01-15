using UnityEngine;
using System.Collections;

namespace Sjabloon
{
    public class Screenshake : MonoBehaviour
    {
        private Vector3 m_DefaultPosition;

        private void Start()
        {
            m_DefaultPosition = transform.position.Copy();
        }

        private void OnDestroy()
        {

        }

        public void StartShake(float strength, float length)
        {
            StopShake();
            StartCoroutine(ScreenshakeRoutine(strength, length));
        }

        public void StopShake()
        {
            StopCoroutine("ScreenshakeRoutine");
            transform.position = m_DefaultPosition;
        }

        private IEnumerator ScreenshakeRoutine(float initialStrength, float length)
        {
            float timer = length;
            float strength = initialStrength;

            while (timer > 0)
            {
                timer -= Time.deltaTime;
                strength = Mathf.Lerp(0.0f, initialStrength, timer);

                float shakeX = Random.Range(-strength, strength);
                float shakeY = Random.Range(-strength, strength);

                transform.position = new Vector3(m_DefaultPosition.x + shakeX,
                                                 m_DefaultPosition.y + shakeY,
                                                 m_DefaultPosition.z);

                yield return new WaitForEndOfFrame();
            }

            transform.position = m_DefaultPosition;
        }
    }
}
