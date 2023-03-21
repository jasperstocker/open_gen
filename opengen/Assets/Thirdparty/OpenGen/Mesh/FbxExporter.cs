using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


namespace opengen.mesh
{
    public class FBXExporter
    {
        private static DynamicMesh exportDynamicMesh;
        private static string targetFolder = "";
        private static string targetName = "ExportedFBX";

        //ID numbers to connect the various parts of the FBX data
        public static int geomIdent = 10000;
        public static int modelIdent = 20000;
        public static int textureIdent = 30000;
        public static int matieralIdent = 40000;

        public static float SCALE = 1.0f;
        public static bool X_FLIP = false;
        public static bool Y_FLIP = false;
        public static bool Z_FLIP = false;

        public static bool Export(string filename, DynamicMesh dynamicMesh)//, ExportMaterial[] textures, bool copyTextures)
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = $"MyBuilding_{DateTime.Now:yy_MM_dd_HH_mm_ss}";
            }

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            folder = Path.Combine(folder, "VU.CITY/Exported");
            if (!CreateFolder(folder))
            {
                throw new Exception("Cannot save to target folder " + folder);
            }
            folder = folder.Replace(" ", "");
            filename = filename.Replace(" ", "");

            exportDynamicMesh = dynamicMesh;
            targetFolder = folder;
            targetName = filename;
            
            DynamicMeshToFile(targetFolder, targetName);

            exportDynamicMesh = null;

            return true;
        }

        public static string Export(DynamicMesh dynamicMesh)//, ExportMaterial[] textures)
        {            
            exportDynamicMesh = dynamicMesh;
            //exportTextures = textures;
            targetFolder = "";

            string output = DynamicMeshToString();

            exportDynamicMesh = null;
            //exportTextures = null;

            return output;
        }


        private static bool CreateFolder(string newDirectory)
        {
            if (Directory.Exists(newDirectory))
            {
                return true;
            }

            try
            {
                Directory.CreateDirectory(newDirectory);
            }
            catch
            {
                throw new Exception("Error!, Failed to create target folder! " + newDirectory);
            }

            return true;
        }

        private static void DynamicMeshToFile(string folder, string filename)
        {
//            string specialFilename = string.Format("{0}_{1:yy_MM_dd_hh_mm_ss}", filename, DateTime.Now);
            using (StreamWriter sw = new(folder + "/" + filename + ".fbx"))
            {
                sw.Write(DynamicMeshToString());
            }
        }

        private static string DynamicMeshToString()
        {
            StringBuilder sb = new();

            //Header
            sb.AppendLine("; FBX 7.2.0 project file");
            sb.AppendLine("; Copyright (C) 1997-2010 Autodesk Inc. and/or its licensors.");
            sb.AppendLine("; All rights reserved.");
            sb.AppendLine("; ----------------------------------------------------");

            sb.AppendLine("FBXHeaderExtension:  {");
            sb.AppendLine("	FBXHeaderVersion: 1003");
            sb.AppendLine("	FBXVersion: 7200");
            sb.AppendLine("	Creator: \"ArchidynamicMesh FBX Exporter\"");
            sb.AppendLine("}");

            sb.AppendLine("; Object definitions");
            sb.AppendLine(";------------------------------------------------------------------");

            sb.AppendLine("Definitions:  {");
            sb.AppendLine("	Version: 100");
            sb.AppendLine("	Count: 4");
            sb.AppendLine("	ObjectType: \"Geometry\" {");
            sb.AppendLine("		Count: 1");
            sb.AppendLine("		PropertyTemplate: \"FbxDynamicMesh\" {");
            sb.AppendLine("		}");
            sb.AppendLine("	}");
            sb.AppendLine("	ObjectType: \"Material\" {");
            sb.AppendLine("		Count: 1");
            sb.AppendLine("		PropertyTemplate: \"FbxSurfacePhong\" {");
            sb.AppendLine("		}");
            sb.AppendLine("	}");
            sb.AppendLine("	ObjectType: \"Texture\" {");
            sb.AppendLine("		Count: 1");
            sb.AppendLine("		PropertyTemplate: \"FbxFileTexture\" {");
            sb.AppendLine("		}");
            sb.AppendLine("	}");
            sb.AppendLine("	ObjectType: \"Model\" {");
            sb.AppendLine("		Count: 1");
            sb.AppendLine("		PropertyTemplate: \"FbxNode\" {");
            sb.AppendLine("		}");
            sb.AppendLine("	}");
            sb.AppendLine("}");

            sb.AppendLine("; Object properties");
            sb.AppendLine(";------------------------------------------------------------------");

            int numberOfSubdynamicMeshes = exportDynamicMesh.subMeshCount;

            sb.AppendLine("Objects:  {");
            for (int sm = 0; sm < numberOfSubdynamicMeshes; sm++)
            {
                //recompile data lists by subdynamicMesh
                int[] Tris = exportDynamicMesh.subTriangles[sm].ToArray();
                List<int> SMTris = new();
                List<Vector3> SMVerts = new();
                List<Vector3> SMNorms = new();
                List<Vector2> SMUVs = new();
                int newIndex = 0;
                foreach (int index in Tris)
                {
                    Vector3 vert = exportDynamicMesh.vertices[index];
                    SMVerts.Add(vert);
                    SMUVs.Add(exportDynamicMesh.uvs[index]);
                    SMNorms.Add(exportDynamicMesh.normals[index]);
                    SMTris.Add(newIndex);
                    newIndex++;
                }

                //GEOMETRY DATA
                int vertCount = SMVerts.Count;
                sb.AppendLine("	Geometry: " + (geomIdent + sm) + ", \"Geometry::\", \"DynamicMesh\" {");
                sb.AppendLine("		Vertices: *" + (vertCount * 3) + " {");
                sb.Append("			a: ");

                float dimScale = 100 * SCALE;
                float xFlip = X_FLIP ? -1 : 1;
                float yFlip = Y_FLIP ? -1 : 1;
                float zFlip = Z_FLIP ? -1 : 1;
                bool faceFlip = (int)(xFlip * yFlip * zFlip) == -1;
                for (int i = 0; i < vertCount; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }

                    Vector3 wv = SMVerts[i];
                    sb.Append(string.Format("{0},{1},{2}", wv.x * dimScale * xFlip, wv.y * dimScale * yFlip, wv.z * dimScale * zFlip));//invert the x and scale up by 100* as Unity FBX default is 0.01
                }
                sb.AppendLine("");
                sb.AppendLine("		}  ");

                int triCount = SMTris.Count;
                sb.AppendLine("		PolygonVertexIndex: *" + triCount + " { ");
                sb.Append("			a:  ");
                for (int i = 0; i < triCount; i += 3)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }

                    int a, b, c;

                    if(faceFlip)
                    {
                        a = SMTris[i];
                        b = SMTris[i + 2];
                        c = -SMTris[i + 1] - 1;//oh look at that - isn't that an interesting way of denoting the end of a polygon...
                        //NOTE: we reversed the x for we need to rewind the triangles
                    }
                    else
                    {
                        a = SMTris[i];
                        b = SMTris[i + 1];
                        c = -SMTris[i + 2] - 1;
                    }


                    sb.Append(string.Format("{0},{1},{2}", a, b, c));//
                }
                sb.AppendLine("");
                sb.AppendLine("		} ");

                sb.AppendLine("		GeometryVersion: 124");
                sb.AppendLine("		LayerElementNormal: 0 {");
                sb.AppendLine("			Version: 101");
                sb.AppendLine("			Name: \"\"");
                sb.AppendLine("			MappingInformationType: \"ByPolygonVertex\"");
                sb.AppendLine("			ReferenceInformationType: \"Direct\"");
                sb.AppendLine("			Normals: *" + (triCount * 3) + " {");
                sb.Append("					a:  ");

                //map out the normals by polyvertex so we'll use the triangle array to get all the normal values
                //also - need to reverse the x value as we've x-flipped the model
                for (int i = 0; i < triCount; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }

                    Vector3 nrm = SMNorms[SMTris[i]];
                    sb.Append(string.Format("{0},{1},{2}", nrm.x * xFlip, nrm.y * yFlip, nrm.z * zFlip));
                }
                sb.AppendLine("");
                sb.AppendLine("			} ");
                sb.AppendLine("		}");

                int UVCount = SMUVs.Count;
                sb.AppendLine("		LayerElementUV: 0 {");
                sb.AppendLine("			Version: 101");
                sb.AppendLine("			Name: \"map1\"");
                sb.AppendLine("			MappingInformationType: \"ByPolygonVertex\"");
                sb.AppendLine("			ReferenceInformationType: \"IndexToDirect\"");

                sb.AppendLine("			UV: *" + (UVCount * 2) + " {");
                sb.Append("				a:");
                for (int i = 0; i < UVCount; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }

                    Vector2 smuv = SMUVs[i];
                    sb.Append(string.Format("{0},{1}", smuv.x, smuv.y));
                }
                sb.AppendLine("");
                sb.AppendLine("			} ");

                sb.AppendLine("			UVIndex: *" + triCount + " {");
                sb.Append("				a:");
                for (int i = 0; i < triCount; i += 3)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }

                    int a = SMTris[i];
                    int b = SMTris[i + 2];
                    int c = SMTris[i + 1];
                    //rewind triangles because of x flip

                    if(faceFlip)
                    {
                        sb.Append(string.Format("{0},{1},{2}", a, b, c));//
                    }
                    else
                    {
                        sb.Append(string.Format("{0},{1},{2}", a, c, b));//
                    }
                }
                sb.AppendLine("");

                sb.AppendLine("			} ");
                sb.AppendLine("		} ");

                sb.AppendLine("		LayerElementMaterial: 0 {");
                sb.AppendLine("			Version: 101");
                sb.AppendLine("			Name: \"\"");
                sb.AppendLine("			MappingInformationType: \"AllSame\"");
                sb.AppendLine("			ReferenceInformationType: \"IndexToDirect\"");
                sb.AppendLine("			Materials: *1 {");
                sb.AppendLine("				a: 0");
                sb.AppendLine("			} ");
                sb.AppendLine("		}");
                sb.AppendLine("		Layer: 0 {");
                sb.AppendLine("			Version: 100");
                sb.AppendLine("			LayerElement:  {");
                sb.AppendLine("				Type: \"LayerElementNormal\"");
                sb.AppendLine("				TypedIndex: 0");
                sb.AppendLine("			}");
                sb.AppendLine("			LayerElement:  {");
                sb.AppendLine("				Type: \"LayerElementMaterial\"");
                sb.AppendLine("				TypedIndex: 0");
                sb.AppendLine("			}");
                sb.AppendLine("			LayerElement:  {");
                sb.AppendLine("				Type: \"LayerElementUV\"");
                sb.AppendLine("				TypedIndex: 0");
                sb.AppendLine("			}");
                sb.AppendLine("		}");
                sb.AppendLine("	}");
            }

            //MODELS
            // int count = 0;
//             foreach (ExportMaterial eMat in exportTextures)
//             {
//                 sb.AppendLine("	Model: " + (modelIdent + count) + ", \"Model::" + targetName + ":\", \"DynamicMesh\" {");
//                 //                sb.AppendLine("	Model: "+(modelIdent+count)+", \"Model::"+targetName+":"+eMat.name+"\", \"DynamicMesh\" {");
//                 sb.AppendLine("		Version: 232");
//                 sb.AppendLine("		Properties70:  {");
//                 sb.AppendLine("			P: \"RotationActive\", \"bool\", \"\", \"\",1");
//                 sb.AppendLine("			P: \"InheritType\", \"enum\", \"\", \"\",1");
//                 sb.AppendLine("			P: \"ScalingMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
//                 sb.AppendLine("			P: \"DefaultAttributeIndex\", \"int\", \"Integer\", \"\",0");
// #if UNITY_EDITOR
//                 if (!eMat.generated && eMat.material.HasProperty("_BumpMap"))
//                 {
//                     string bumpMapFalePath = UnityEditor.AssetDatabase.GetAssetPath(eMat.material.GetTexture("_BumpMap"));
//                     sb.AppendLine("			P: \"NormalMap\", \"Enum\", \"A+U\",0, \"" + bumpMapFalePath + "\"");
//                 }
// #endif
//                 sb.AppendLine("		}");
//                 sb.AppendLine("		Shading: T");
//                 sb.AppendLine("		Culling: \"CullingOff\"");
//                 sb.AppendLine("	}");
//                 count++;
//             }

            //MATERIALS
            // count = 0;
            // foreach (ExportMaterial eMat in exportTextures)
            // {
            //     sb.AppendLine("	Material: " + (matieralIdent + count) + ", \"Material::" + eMat.name + "\", \"\" {");
            //     //                sb.AppendLine("	Material: "+(matieralIdent+count)+", \"Material::"+targetName+":"+eMat.name+"F\", \"\" {");
            //     sb.AppendLine("		Version: 102");
            //     sb.AppendLine("		ShadingModel: \"phong\"");
            //     sb.AppendLine("		Properties70:  {");
            //     Color diffCol = eMat.material != null ? eMat.material.color : Color.white;
            //     sb.AppendLine("			P: \"Diffuse\", \"Vector3D\", \"Vector\", \"\"," + diffCol.r + "," + diffCol.g + "," + diffCol.b);
            //     sb.AppendLine("			P: \"DiffuseColor\", \"ColorRGB\", \"Color\", \" \"," + diffCol.r + "," + diffCol.g + "," + diffCol.b);
            //
            //     if (eMat.material != null && eMat.material.HasProperty("_SpecColor"))
            //     {
            //         Color specCol = eMat.material.GetColor("_SpecColor");
            //         sb.AppendLine("			P: \"Specular\", \"Vector3D\", \"Vector\", \"\"," + specCol.r + "," + specCol.g + "," + specCol.b);
            //         sb.AppendLine("			P: \"SpecularColor\", \"ColorRGB\", \"Color\", \" \"," + specCol.r + "," + specCol.g + "," + specCol.b);
            //     }
            //
            //     if (eMat.material != null && eMat.material.HasProperty("_Shininess"))
            //     {
            //         sb.AppendLine("			P: \"Shininess\", \"float\", \"Number\", \"\"," + eMat.material.Getfloat("_Shininess"));
            //     }
            //     sb.AppendLine("		}");
            //     sb.AppendLine("	}");
            //     count++;
            // }

            //TEXTURES
            //If selected - export the textures to the export folder
            //Add the textures to the 
            // count = 0;
            // foreach (ExportMaterial eMat in exportTextures)
            // {
            //     string destinationFile = eMat.filepath;
            //     string texturePath = destinationFile;
            //     if (_copyTextures)
            //     {
            //         if (!eMat.generated && destinationFile != null)
            //         {
            //             int stripIndex = destinationFile.LastIndexOf('/');
            //             if (stripIndex >= 0)
            //                 destinationFile = destinationFile.Substring(stripIndex + 1).Trim();
            //             texturePath = destinationFile;//relative file path when using textures in the export folder
            //             string folder = targetFolder;
            //             destinationFile = folder + destinationFile;
            //             if (!eMat.generated)
            //             {
            //                 try
            //                 {
            //                     //Copy the source file
            //                     //Debug.Log("Copying texture from " + kvp.Value.filepath + " to " + destinationFile);
            //                     File.Copy(eMat.filepath, destinationFile, true);
            //                 }
            //                 catch
            //                 {
            //                     throw new Exception("Could not copy texture!");
            //                 }
            //             }
            //         }
            //         else
            //         {
            //             try
            //             {
            //                 Material mat = eMat.material;
            //                 if (mat != null)
            //                 {
            //                     string textureFilename = string.Format("{0}.png", eMat.name);
            //                     string textureFullPath = Path.Combine(targetFolder, textureFilename);
            //                     Texture2D texture = eMat.material.mainTexture as Texture2D;
            //                     if (texture == null) continue;
            //
            //                     RenderTexture renderTex = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            //
            //                     Graphics.Blit(texture, renderTex);
            //                     RenderTexture previous = RenderTexture.active;
            //                     RenderTexture.active = renderTex;
            //                     Texture2D readableText = new Texture2D(texture.width, texture.height);
            //                     readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            //                     readableText.Apply();
            //                     RenderTexture.active = previous;
            //                     RenderTexture.ReleaseTemporary(renderTex);
            //
            //                     byte[] bytes = readableText.EncodeToPNG();
            //                     File.WriteAllBytes(textureFullPath, bytes);
            //
            //                     destinationFile = textureFullPath;
            //                     texturePath = textureFilename;
            //                 }
            //             }
            //             catch (Exception e)
            //             {
            //                 Console.WriteLine(string.Format("Texture save skipped with following error: {0}", e));
            //                 throw;
            //             }
            //         }
            //     }
            //
            //     if(eMat.material == null || eMat.material.mainTexture == null) continue;
            //
            //     //                sb.AppendLine("	Texture: " + (textureIdent + count) + ", \"Texture::" + targetName + ":" + eMat.name + "2F\", \"\" {");
            //     sb.AppendLine("	Texture: " + (textureIdent + count) + ", \"Texture::" + eMat.name + "\", \"\" {");
            //     sb.AppendLine("		Type: \"TextureVideoClip\"");
            //     sb.AppendLine("		Version: 202");
            //     //                sb.AppendLine("		TextureName: \"Texture::" + targetName + ":" + eMat.name + "2F\"");
            //     sb.AppendLine("		TextureName: \"Texture::" + eMat.name + "\"");
            //     sb.AppendLine("		FileName: \"" + destinationFile + "\"");
            //     sb.AppendLine("		RelativeFilename: \"" + texturePath + "\"");
            //     sb.AppendLine("	}");
            //     count++;
            // }
            sb.AppendLine("}");

            //CONNECTIONS
            //This part defines how all the data objects connect to make the model
            sb.AppendLine("; Object connections");
            sb.AppendLine(";------------------------------------------------------------------");

            sb.AppendLine("Connections:  {");

            // int conCount = 0;
            // foreach (ExportMaterial eMat in exportTextures)
            // {
            //     sb.AppendLine("	;Model::" + targetName + ", Model::RootNode");
            //     sb.AppendLine("	C: \"OO\"," + (modelIdent + conCount) + ",0");
            //     sb.AppendLine("	");
            //
            //     if(eMat.material != null && eMat.material.mainTexture != null)
            //     {
            //         sb.AppendLine("	;Texture::" + targetName + ", Material:" + eMat.name);
            //         sb.AppendLine("	C: \"OP\"," + (textureIdent + conCount) + "," + (matieralIdent + conCount) + ", \"DiffuseColor\"");
            //         sb.AppendLine("	");
            //     }
            //     
            //     sb.AppendLine("	;Geometry::, Model::" + targetName);
            //     sb.AppendLine("	C: \"OO\"," + (geomIdent + conCount) + "," + (modelIdent + conCount) + "");
            //     sb.AppendLine("	");
            //
            //     sb.AppendLine("	;Material::" + eMat.name + ", Model::" + targetName);
            //     sb.AppendLine("	C: \"OO\"," + (matieralIdent + conCount) + "," + (modelIdent + conCount) + "");
            //     sb.AppendLine("	");
            //     conCount++;
            // }
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static bool CreateTargetFolder()
        {
            string newDirectory = targetFolder;
#if UNITY_EDITOR
            if (System.IO.Directory.Exists(newDirectory))
            {
                if (UnityEditor.EditorUtility.DisplayDialog("File directory exists", "Are you sure you want to overwrite the contents of this file?", "Cancel", "Overwrite"))
                {
                    return false;
                }
            }
#endif

            try
            {
                System.IO.Directory.CreateDirectory(newDirectory);
            }
            catch
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog("Error!", "Failed to create target folder!", "");
#else
                throw new Exception("FBX Export CreateTargetFolder: Failed to create target folder!");
#endif
                return false;
            }

            return true;
        }
    }

    // public struct ExportMaterial
    // {
    //     public string name;
    //     public Material material;
    //     public string filepath;
    //     public bool generated;
    // }
}

//This FBX Exporter isn't a fully functional one
//For the purposes this will only export basic geometry
//There is no support for animation and rigging
//TESTED IN
//Unity3D, Maya