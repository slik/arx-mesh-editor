﻿using Assets.Scripts.ArxLevelEditor;
using Assets.Scripts.ArxLevelEditor.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.ArxLevelEditor.Editing
{
    public class PolygonSelector : MonoBehaviour
    {
        GameObject currentlySelected = null;

        int selectLayer;

        private void Awake()
        {
            selectLayer = 1 << LayerMask.NameToLayer("EditableLevelMesh");
        }

        void Deselect()
        {
            Gizmo.Detach();
            Gizmo.Visible = false;

            //place currently selected back into mesh
            if (currentlySelected != null)
            {
                var selectedPrimitive = currentlySelected.GetComponent<EditablePrimitive>();
                var editableMesh = LevelEditor.CurrentLevel.EditableLevelMesh.GetMaterialMesh(selectedPrimitive.material);
                editableMesh.AddPrimitive(selectedPrimitive.info);
                editableMesh.UpdateMesh();
                Destroy(currentlySelected);
                currentlySelected = null;
            }
        }

        private void Update()
        {
            if (EditWindow.MouseInEditWindow)
            {
                if (LevelEditor.EditState == EditState.Polygons && Input.GetMouseButtonDown(0))
                {
                    //convert mouse position to viewport position
                    var ray = EditWindow.GetRayFromMousePosition();
                    //raycast with levelmeshes to see what polygon was clicked
                    if (Physics.Raycast(ray, out RaycastHit hitInfo, float.MaxValue, selectLayer))
                    {
                        var mesh = hitInfo.transform.gameObject.GetComponent<MaterialMesh>();
                        if (mesh != null)
                        {
                            Deselect();

                            //remove newly selected from mesh
                            var primitive = mesh.GetByTriangleIndex(hitInfo.triangleIndex);
                            mesh.RemovePrimitive(primitive);
                            mesh.UpdateMesh();

                            var go = new GameObject();
                            currentlySelected = go;
                            go.transform.SetParent(mesh.gameObject.transform);
                            go.transform.localPosition = Vector3.zero;
                            go.transform.localScale = Vector3.one;
                            var editablePrimitive = go.AddComponent<EditablePrimitive>();
                            editablePrimitive.UpdatePrimitive(primitive, mesh.EditorMaterial);

                            //TODO: add some indicator this poly is selected
                        }
                    }
                    else
                    {
                        Deselect();
                    }
                }
            }
        }
    }
}
