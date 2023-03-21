using opengen.types;
using UnityEditor;
using UnityEngine;

namespace opengen.unityeditor
{
    [CustomEditor(typeof(ShapeComponent))]
    public class ShapeEditor : Editor
    {
	    private ShapeComponent _component;
		
        private void OnEnable()
        {
	        _component = (ShapeComponent) target;
        }

        public override void OnInspectorGUI()
        {
	        if (GUILayout.Button("Generate Triangle"))
	        {
		        _component.GenerateTriangle();
	        }
	        if (GUILayout.Button("Generate Square"))
	        {
		        _component.GenerateSquare();
	        }
	        
	        base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
	        switch(Event.current.type)
	        {
		        case EventType.MouseMove:
			        SceneView.RepaintAll();//this is needed to make sure we get editor scene updates on more than clicks
			        break;
	        }
	        
	        bool controlPress = Event.current.control;
	        
	        Transform transform = _component.transform;
	        Matrix4x4 mat = transform.localToWorldMatrix;
	        Matrix4x4 invMat = mat.inverse;
	        Handles.matrix = mat;
	        
	        Vector3 offset = transform.position;
	        Shape shape = _component.shape;
	        int shapeSize = shape.pointCount;
	        
	        Vector3 mousePos = MousePoint(offset);
	        mousePos = invMat.MultiplyPoint(mousePos);
	        
	        for(int i = 0; i < shapeSize; i++)
	        {
		        Handles.color = _component.colour;
		        int next = i + 1;
		        if(next >= shapeSize)
		        {
			        next = 0;
		        }

		        Vector3 p0 = shape[i].Vector3Flat();
		        Vector3 p1 = shape[next].Vector3Flat();

		        if (_component.drawShape)
		        {
			        Handles.DrawLine(p0, p1);
		        }

		        float handleSize = HandleUtility.GetHandleSize(p0) * 0.13f;

		        if (!controlPress)
		        {
			        Vector3 pos = Handles.FreeMoveHandle(p0, Quaternion.identity, handleSize, Vector3.zero, Handles.CircleHandleCap);
			        if (Vector3.Distance(p0, mousePos) > Mathf.Epsilon && Vector3.Distance(p0, pos) > Mathf.Epsilon)
			        {
				        shape[i] = mousePos.Vector2Flat();
			        }
		        
			        Vector3 pC = Vector3.Lerp(p0, p1, 0.5f);
			        float mouseDist = Vector3.Distance(mousePos, pC);
			        float lineDist = Vector3.Distance(p0, p1);
			        if (mouseDist < lineDist * 0.2f)
			        {
				        float newPointSize = HandleUtility.GetHandleSize(pC) * 0.13f;
				        Handles.color = Color.green;
				        if(Handles.Button(pC, GetBillboardRotation(pC), newPointSize, newPointSize, Handles.CircleHandleCap))
				        {
					        shape.Insert(next, mousePos.Vector2Flat());
				        }
			        }
		        }
		        else
		        {
			        float newPointSize = HandleUtility.GetHandleSize(p0) * 0.13f;
			        Handles.color = Color.red;
			        if(Handles.Button(p0, GetBillboardRotation(p0), newPointSize, newPointSize, Handles.CircleHandleCap))
			        {
				        shape.Remove(i);
				        break;
			        }
		        }
	        }
        }
        
        private Vector3 MousePoint(Vector3 origin)
        {
	        Camera cam = Camera.current;
            
	        if (!cam)
	        {
		        return Vector3.zero;
	        }

	        if(cam.pixelHeight == 0)//if your mouse dilly dallies outside of the scene view - good luck mate!
	        {
		        return Vector3.zero;//or we could cut that out right now
	        }

	        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
	        Vector3 output = Vector3.zero;

	        float currentDist = Mathf.Infinity;
	        RaycastHit hitInfo;
	        if (Physics.Raycast(ray, out hitInfo))
	        {
		        output = hitInfo.point;
		        currentDist = Vector3.Distance(ray.origin, output);
	        }

	        Plane buildingPlane = new Plane(Vector3.up, origin);
	        float mouseRayDistance;
	        if (buildingPlane.Raycast(ray, out mouseRayDistance))
	        {
		        if (currentDist > mouseRayDistance)
		        {
			        output = ray.GetPoint(mouseRayDistance);
		        }
	        }

	        bool snapToGrid = Event.current.shift;
	        if (snapToGrid)
	        {
		        output.x = Mathf.Round(output.x*10)/10;
		        output.z = Mathf.Round(output.z*10)/10;
	        }

	        return output;
        }

        private Quaternion GetBillboardRotation(Vector3 position)
        {
	        Camera cam = Camera.current;
            
	        if (!cam)
	        {
		        return Quaternion.identity;
	        }

	        if(cam.pixelHeight == 0)//if your mouse dilly dallies outside of the scene view - good luck mate!
	        {
		        return Quaternion.identity;//or we could cut that out right now
	        }
	        
	        return Quaternion.LookRotation(position - cam.transform.position, Vector3.up);
        }
    }
}