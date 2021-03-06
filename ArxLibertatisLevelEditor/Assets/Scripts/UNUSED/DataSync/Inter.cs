﻿using Assets.Scripts.ArxLevelEditor;
using Assets.Scripts.ArxNative.IO;
using Assets.Scripts.ArxNative.IO.DLF;
using Assets.Scripts.ArxNative.IO.FTL;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.DataSync
{
    public class Inter : MonoBehaviour
    {
        public void LoadFrom(Level level, int index)
        {
            DLF_IO_INTER inter = level.ArxLevelNative.DLF.inters[index];

            name = ArxIOHelper.GetString(inter.name);

            string interPath = name.ToLowerInvariant();
            int graphPos = interPath.IndexOf("graph");
            interPath = interPath.Substring(graphPos);
            int lastDot = interPath.LastIndexOf('.');
            interPath = interPath.Substring(0, lastDot);

            var ftlPath = Path.Combine(EditorSettings.DataDir, "game", ArxIOHelper.ArxPathToPlatformPath(interPath + ".ftl"));
            if (File.Exists(ftlPath))
            {
                FTL_IO ftl = new FTL_IO();
                using (var fs = new FileStream(ftlPath, FileMode.Open, FileAccess.Read))
                using (var unp = FTL_IO.EnsureUnpacked(fs))
                {
                    ftl.ReadFrom(unp);
                }

                var mesh = ftl.CreateMesh();
                var mf = gameObject.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;
                var mr = gameObject.AddComponent<MeshRenderer>();
                mr.sharedMaterial = MaterialsDatabase.TEST;
            }

            //depending on what the path is load item, npc, fix, camera or marker
            if (interPath.Contains("items"))
            {

            }
            else if (interPath.Contains("npc"))
            {

            }
            else if (interPath.Contains("fix"))
            {

            }
            else if (interPath.Contains("camera"))
            {

            }
            else if (interPath.Contains("marker"))
            {

            }

            transform.localPosition = inter.pos.ToVector3();
            transform.localEulerAngles = inter.angle.ToEuler();
        }
    }
}
