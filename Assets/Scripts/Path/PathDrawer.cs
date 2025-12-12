/*--------------------------------------------------------------------------------*
  File Name: PathDrawer.cs
  Authors: Sam Friedman

  Copyright DigiPen Institute of Technology
 *--------------------------------------------------------------------------------*/

using System.Collections.Generic;
using UnityEngine;

namespace GestureNX
{
    public class PathDrawer : MonoBehaviour
    {
        [SerializeField]
        private Camera cam;

        [SerializeField]
        private LineRenderer lineRendererPrefab;

        [SerializeField]
        private GestureHolder gestureHolder;

        private float lineRendererOffsetZ = -0.05f;

        private LineRenderer lineRenderer;
        private readonly List<Vector3> points = new();
        private bool isValidDrawCurrentStrip = false;

        // Update is called once per frame
        private void Update()
        {
            if (PathInputManager.Instance.IsPressed())
            {
                CreateLineStrip();
                gestureHolder.RequestStart();
            }
            else if (PathInputManager.Instance.IsDown())
            {
                TryAddPoint();
            }
        }

        // OnValidate is called when the script is loaded or a value is changed in the inspector
        private void OnValidate()
        {
            if (cam == null)
            {
                cam = Camera.main;
            }
        }

        private void CreateLineStrip()
        {
            lineRenderer = Instantiate(lineRendererPrefab);
            lineRenderer.transform.SetParent(transform, true);

            points.Clear();

            isValidDrawCurrentStrip = true;
        }

        private void UpdateLineStrip()
        {
            if (lineRenderer == null)
            {
                return;
            }

            if (points.Count <= 0)
            {
                return;
            }

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }

        private void TryAddPoint()
        {
            if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                isValidDrawCurrentStrip = false;
                return;
            }

            if (!hit.collider.CompareTag("Drawable"))
            {
                isValidDrawCurrentStrip = false;
                return;
            }

            if (!isValidDrawCurrentStrip)
            {
                CreateLineStrip();
                return;
            }

            Vector3 forwardOffset = Vector3.forward * lineRendererOffsetZ;
            Vector3 point = hit.point + forwardOffset;

            points.Add(point);
            gestureHolder.RequestUpdate(point);
            UpdateLineStrip();
        }

        public GestureKind Evaluate()
        {
            return gestureHolder.RequestEnd();
        }

        public void Clear()
        {
            if (transform.childCount == 0)
            {
                return;
            }
            
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
        }
    }
}
