using System;
using UnityEngine;

public class MagnifyingObject : MonoBehaviour
{
   Renderer _renderer;
   Camera _camera;

   void Start()
   {
      _renderer = GetComponent<Renderer>();
      _camera = Camera.main;
   }

   private void Update()
   {
      Vector3 screenPoint = _camera.WorldToScreenPoint(transform.position);
      screenPoint.x = screenPoint.x / Screen.width;
      screenPoint.y = screenPoint.y / Screen.height;
      _renderer.material.SetVector("_ObjScreenPos", screenPoint);
      
   }
}
