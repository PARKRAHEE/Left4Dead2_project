using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;

    public Camera fpsCam;

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) // 마우스 좌클릭
        {
            Debug.Log("클릭됨");
            Shoot();
        }
    }

    void Shoot()
    {
        Debug.Log("데미지 들어감: " + damage);

        RaycastHit hit;

        Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * range, Color.red, 1f);

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("맞은 대상: " + hit.transform.name);

            ZombieBase zombie = hit.transform.GetComponentInParent<ZombieBase>();

            if (zombie != null)
            {
                Debug.Log("좀비 맞음!");
                zombie.TakeDamage(damage);
            }
            else
            {
                Debug.Log("좀비 아님");
            }
        }
    }
}
