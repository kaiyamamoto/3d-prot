using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animTest : MonoBehaviour
{

    [SerializeField]
    private DynamicAnimator _target;

    // Use this for initialization
    void Start()
    {
        _target.Setup();
    }
}
