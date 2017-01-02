using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitAnimController : MonoBehaviour
{
    public Animator Animator;

    private NavMeshAgent NavAgent;

    // Use this for initialization
    void Start ()
    {
        NavAgent = GetComponent<NavMeshAgent>();

        if (Animator == null)
            Debug.Log("ERROR: No animator in " + gameObject.name + "!");
    }
	
	// Update is called once per frame
	void Update ()
    {
        Animator.SetFloat("Speed", NavAgent.velocity.magnitude);
	}
}
