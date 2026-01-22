using System;
using UnityEngine;

public class DetectionSphere : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        var interactible = other.GetComponent<Interactible>();
        if (interactible != null)
        {
            //other.GetComponent<Outline>().enabled = true;
            if (FindFirstObjectByType<PlayerController>().InteractAction.action.IsPressed())
            {
                interactible.Interact();
            }
        }

        if (other.GetComponent<Pickable>())
        {
            if (other.GetComponent<Item>())
            {
                CollisionSystem.Instance.HandleItemPickup(this.gameObject, other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Interactible interactible = other.GetComponent<Interactible>();
        if (interactible != null)
        {
            //other.GetComponent<Outline>().enabled = false;
        }
    }
}