﻿using Assets.Scripts.ArxLevel;
using Assets.Scripts.ArxLevelEditor;
using Assets.Scripts.ArxLevelEditor.Material;
using Assets.Scripts.ArxLevelEditor.Mesh;
using Assets.Scripts.ArxNative.IO;
using Assets.Scripts.ArxNative.IO.Shared_IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.ArxLevelLoading
{
    public static class LevelSaver
    {
        public static void SaveLevel(Level level, string name)
        {
            var lvln = level.ArxLevelNative;

            Vector3 camPos = LevelEditor.EditorCamera.transform.position * 100;
            camPos.y *= -1;
            lvln.DLF.header.positionEdit = new SavedVec3(camPos);
            lvln.DLF.header.angleEdit = new SavedAnglef(LevelEditor.EditorCamera.transform.eulerAngles);
            lvln.DLF.header.offset = new SavedVec3(level.LevelOffset);

            SaveMesh(level);

            lvln.SaveLevel(name);
        }

        class LevelCell
        {
            public readonly int x, z;
            public readonly List<Tuple<EditorMaterialKey, EditablePrimitive>> primitives = new List<Tuple<EditorMaterialKey, EditablePrimitive>>();

            public LevelCell(int x, int z)
            {
                this.x = x;
                this.z = z;
            }

            public void AddPrimitive(EditorMaterialKey mat, EditablePrimitive prim)
            {
                primitives.Add(new Tuple<EditorMaterialKey, EditablePrimitive>(mat, prim));
            }
        }

        static Vector2Int GetPrimitiveCellPos(EditablePrimitive prim)
        {
            Vector2 pos = Vector2.zero;

            foreach (var v in prim.vertices)
            {
                pos += new Vector2(v.position.x, v.position.z);
            }
            pos /= prim.vertices.Length;
            return new Vector2Int((int)(pos.x / 100), (int)(pos.y / 100));
        }

        static void SaveMesh(Level level)
        {
            // create texture containers

            HashSet<string> uniqueTexturePaths = new HashSet<string>();

            foreach (var kv in level.EditableLevelMesh.MaterialMeshes)
            {
                if (kv.Value.PrimitiveCount > 0) //only add if we have primitives for this material
                {
                    //TODO: remove that later
                    //var texPath = kv.Key.TexturePath.Replace(EditorSettings.DataDir, "");
                    uniqueTexturePaths.Add(kv.Key.TexturePath);
                }
            }

            var fts = level.ArxLevelNative.FTS;
            fts.textureContainers = new ArxNative.IO.FTS.FTS_IO_TEXTURE_CONTAINER[uniqueTexturePaths.Count];
            int i = 0; //nothing speaks against just using a normal index for tc, i dont know why they ever used random ints
            Dictionary<string, int> texPathToTc = new Dictionary<string, int>();
            foreach (var path in uniqueTexturePaths)
            {
                var texPath = path.Replace(EditorSettings.DataDir, "");
                fts.textureContainers[i].fic = ArxIOHelper.GetBytes(texPath, 256);
                texPathToTc[path] = fts.textureContainers[i].tc = i++;
            }

            //put primitives into cells
            int sizex = fts.sceneHeader.sizex;
            int sizez = fts.sceneHeader.sizez;
            LevelCell[] cells = new LevelCell[sizex * sizez];
            for (int z = 0; z < sizez; z++)
            {
                for (int x = 0; x < sizex; x++)
                {
                    int index = ArxIOHelper.XZToCellIndex(x, z, sizex, sizez);
                    var cell = new LevelCell(x, z);
                    cells[index] = cell;
                }
            }


            //add primitives to cells
            Vector2Int maxPos = new Vector2Int(sizex - 1, sizez - 1); //if clamp is inclusive we have to sub 1
            foreach (var kv in level.EditableLevelMesh.MaterialMeshes)
            {
                foreach (var prim in kv.Value.Primitives)
                {
                    var cellpos = GetPrimitiveCellPos(prim);
                    cellpos.Clamp(Vector2Int.zero, maxPos);
                    var cell = cells[ArxIOHelper.XZToCellIndex(cellpos.x, cellpos.y, sizex, sizez)];
                    cell.AddPrimitive(kv.Key, prim);
                }
            }

            List<uint> lightColors = new List<uint>();

            //put primitves into polys in their cells
            for (int z = 0; z < sizez; z++)
            {
                for (int x = 0; x < sizex; x++)
                {
                    int index = ArxIOHelper.XZToCellIndex(x, z, sizex, sizez);
                    var myCell = cells[index];
                    var ftsCell = fts.cells[index];
                    ftsCell.sceneInfo.nbpoly = myCell.primitives.Count;
                    ftsCell.polygons = new ArxNative.IO.FTS.FTS_IO_EERIEPOLY[myCell.primitives.Count];
                    for (i = 0; i < myCell.primitives.Count; i++)
                    {
                        var tup = myCell.primitives[i];
                        var mat = tup.Item1;
                        var prim = tup.Item2;
                        var poly = new ArxNative.IO.FTS.FTS_IO_EERIEPOLY();
                        //copy data that we dont edit yet (but probably should at some point)
                        poly.area = prim.area;
                        poly.norm = new SavedVec3(prim.norm);
                        poly.norm2 = new SavedVec3(prim.norm2);
                        poly.paddy = prim.paddy;
                        poly.room = prim.room;

                        //copy vertices
                        poly.vertices = new ArxNative.IO.FTS.FTS_IO_VERTEX[4];
                        poly.normals = new SavedVec3[4];
                        for (int j = 0; j < prim.vertices.Length; j++)
                        {
                            var vert = prim.vertices[j];
                            var natVert = new ArxNative.IO.FTS.FTS_IO_VERTEX();
                            natVert.posX = vert.position.x;
                            natVert.posY = vert.position.y;
                            natVert.posZ = vert.position.z;

                            natVert.texU = vert.uv.x;
                            natVert.texV = 1 - vert.uv.y;
                            poly.normals[j] = new SavedVec3(vert.normal);
                            lightColors.Add(ArxIOHelper.ToBGRA(vert.color));

                            poly.vertices[j] = natVert;
                        }

                        //set material stuff
                        poly.tex = texPathToTc[mat.TexturePath];//keyerrors should not be possible on this
                        poly.transval = mat.TransVal;
                        poly.type = mat.PolygonType;

                        ftsCell.polygons[i] = poly;
                    }
                }
            }


            var llf = level.ArxLevelNative.LLF;
            llf.lightingHeader.numLights = lightColors.Count;
            llf.lightColors = new uint[lightColors.Count];
            //update llf
            for (i = 0; i < lightColors.Count; i++)
            {
                llf.lightColors[i] = lightColors[i];
            }
        }
    }
}
