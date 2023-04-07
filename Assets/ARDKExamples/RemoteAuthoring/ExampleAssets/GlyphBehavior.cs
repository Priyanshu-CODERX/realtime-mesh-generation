using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlyphBehavior : MonoBehaviour
{

    [SerializeField] private float speed;
    [SerializeField] private float amplitude;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime * 15f);
        transform.Translate(Vector3.up * Mathf.Sin(Time.time) * amplitude);
    }
}
