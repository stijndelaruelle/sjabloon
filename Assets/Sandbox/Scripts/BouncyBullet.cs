using UnityEngine;
using System.Collections;
using Sjabloon;

public class BouncyBullet : Bullet
{
    public override void HadContact(GameObject go)
    {
        transform.up = new Vector3(transform.up.x, transform.up.y * -1, transform.up.z);
    }
}
