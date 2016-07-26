using UnityEngine;
using System.Collections;
using VRTK;
public class BottleBehavior : MonoBehaviour {

    #region forCreatingMesh
    private float heightWithoutNeck = 0.7f;
    private float heightOfNeck = 0.1f;
    private int nbPointsPerRing = 30;
    private int nbRings = 12;
    private float outerRadius = 0.3f;
    private float innerRadius = 0.28f;
    private float outerRadiusNeck = 0.2f;
    private float innerRadiusNeck = 0.15f;
    private float heightOfNeckStart;
    private float heightBetweenEachRing;
    private Vector3[] vertices;
    private float _2pi = Mathf.PI * 2f;
    private int[] triangles;
    private int offsetToInside;
    private int offsetToBottom1;
    private int offsetToBottom2;
    private int offsetToTop1;
    private int offsetToTop2;
    private Mesh mesh;
    #endregion
    #region forInteraction
    private float maxVertexDistanceFromCenter;
    public bool isInteracting = false;
    private int vertexInteractingWith = -1; //this should never be -1 when we're actually using it
    private float distanceThreshold;
    private float[] percentAffectRings; //how much each ring/row is affected by movements made to the attached vertex
    public GameObject interactingController;
    
    #endregion
    // Use this for initialization
    void Start () {
        distanceThreshold= outerRadius / 4.0f;
        vertices = new Vector3[2 * nbPointsPerRing * nbRings + 3 * 2 * nbPointsPerRing];
        //offsetToBottom = 2 * nbPointsPerRing * nbRings;
        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        mesh = filter.mesh;
        mesh.Clear();
        heightBetweenEachRing = heightWithoutNeck / (float)(nbRings - 2);

        #region vertices

        #region outer
        for (int i= 0; i < nbRings - 2; i++) //top 2 rings are bottleneck
        {
            for (int j = 0; j < nbPointsPerRing; j++)
            {
                float x = Mathf.Cos((_2pi / (float)(nbPointsPerRing)) * j) * outerRadius;
                float y = Mathf.Sin((_2pi / (float)(nbPointsPerRing)) * j) * outerRadius;
                vertices[i * nbPointsPerRing + j] = new Vector3(x, y, i * heightBetweenEachRing);
            }
        }
        heightOfNeckStart = heightWithoutNeck + (heightWithoutNeck / (float)(nbRings - 2));
        int nextStartingVertex = (nbRings - 2) * nbPointsPerRing;
        //first ring of bottle neck
        for (int j = 0; j < nbPointsPerRing; j++)
        {
            float x = Mathf.Cos((_2pi / (float)(nbPointsPerRing)) * j) * outerRadiusNeck;
            float y = Mathf.Sin((_2pi / (float)(nbPointsPerRing)) * j) * outerRadiusNeck;
            vertices[nextStartingVertex + j] = new Vector3(x, y, heightOfNeckStart); //same height above previous rings as other rings were
        }
        nextStartingVertex += nbPointsPerRing;
        
        //second ring of the neck
        //Debug.Log("next starting vertex should be blank and is " + vertices[nextStartingVertex]);
        //Debug.Log("The vertex before that one should not be blank and is " + vertices[nextStartingVertex - 1]);
        for (int j = 0; j < nbPointsPerRing; j++)
        {
            float x = Mathf.Cos((_2pi / (float)(nbPointsPerRing)) * j) * outerRadiusNeck;
            float y = Mathf.Sin((_2pi / (float)(nbPointsPerRing)) * j) * outerRadiusNeck;
            vertices[nextStartingVertex + j] = new Vector3(x, y, heightOfNeckStart + heightOfNeck);
        }
        nextStartingVertex += nbPointsPerRing;
        offsetToInside = nextStartingVertex;
        #endregion

        #region inner
        for (int i = 1; i < nbRings-2; i++) //start at 1 because that's where the inner bottom starts. 
        {
            for (int j = 0; j < nbPointsPerRing; j++)
            {
                float x = Mathf.Cos((_2pi / (float)(nbPointsPerRing)) * j) * innerRadius;
                float y = Mathf.Sin((_2pi / (float)(nbPointsPerRing)) * j) * innerRadius;
                vertices[nextStartingVertex + (i-1) * nbPointsPerRing + j] = new Vector3(x, y, i * heightBetweenEachRing);
            }
        }
        nextStartingVertex += nbPointsPerRing * (nbRings - 3);
        //first bottleneck ring
        for (int j = 0; j < nbPointsPerRing; j++)
        {
            float x = Mathf.Cos((_2pi / (float)(nbPointsPerRing)) * j) * innerRadiusNeck;
            float y = Mathf.Sin((_2pi / (float)(nbPointsPerRing)) * j) * innerRadiusNeck;
            vertices[nextStartingVertex + j] = new Vector3(x, y, heightOfNeckStart); //same height above previous rings as other rings were
        }
        //second ring of the neck
        nextStartingVertex += nbPointsPerRing;
        for (int j = 0; j < nbPointsPerRing; j++)
        {
            float x = Mathf.Cos((_2pi / (float)(nbPointsPerRing)) * j) * innerRadiusNeck;
            float y = Mathf.Sin((_2pi / (float)(nbPointsPerRing)) * j) * innerRadiusNeck;
            vertices[nextStartingVertex + j] = new Vector3(x, y, heightOfNeckStart + heightOfNeck);
        }
        nextStartingVertex += nbPointsPerRing;
        #endregion

        #region bottomAndTop
        //ordering is: bottom lower, bottom upper, top outer ring, top inner ring
        //bottom outside
        offsetToBottom1 = nextStartingVertex;
        for (int j=0; j < nbPointsPerRing; j++)
        {
            float x = Mathf.Cos((_2pi / (float)(nbPointsPerRing)) * j) * outerRadius;
            float y = Mathf.Sin((_2pi / (float)(nbPointsPerRing)) * j) * outerRadius;
            vertices[nextStartingVertex + j] = new Vector3(x, y, 0);
        }
        nextStartingVertex += nbPointsPerRing;
        vertices[nextStartingVertex] = new Vector3(0, 0, 0);
        nextStartingVertex += 1;
        //bottom inside
        offsetToBottom2 = nextStartingVertex;
        for (int j = 0; j < nbPointsPerRing; j++)
        {
            float x = Mathf.Cos((_2pi / (float)(nbPointsPerRing)) * j) * innerRadius;
            float y = Mathf.Sin((_2pi / (float)(nbPointsPerRing)) * j) * innerRadius;
            vertices[nextStartingVertex + j] = new Vector3(x, y, heightBetweenEachRing);
        }
        nextStartingVertex += nbPointsPerRing;
        vertices[nextStartingVertex] = new Vector3(0, 0, heightBetweenEachRing);
        nextStartingVertex += 1;
        //top outer ring
        offsetToTop1 = nextStartingVertex;
        for (int j = 0; j < nbPointsPerRing; j++)
        {
            float x = Mathf.Cos((_2pi / (float)(nbPointsPerRing)) * j) * outerRadiusNeck;
            float y = Mathf.Sin((_2pi / (float)(nbPointsPerRing)) * j) * outerRadiusNeck;
            vertices[nextStartingVertex + j] = new Vector3(x, y, heightOfNeckStart + heightOfNeck);
        }
        nextStartingVertex += nbPointsPerRing;
        //top inner ring
        offsetToTop2 = nextStartingVertex;
        for (int j = 0; j < nbPointsPerRing; j++)
        {
            float x = Mathf.Cos((_2pi / (float)(nbPointsPerRing)) * j) * innerRadiusNeck;
            float y = Mathf.Sin((_2pi / (float)(nbPointsPerRing)) * j) * innerRadiusNeck;
            vertices[nextStartingVertex + j] = new Vector3(x, y, heightOfNeckStart + heightOfNeck);
        }
        #endregion
        #endregion

        #region triangles
        int nextStartingIndex = 0;
        int nbTriangleShapesPerLayer = 2 * nbPointsPerRing;
        int nbTrianglePointsSides = 3*(nbTriangleShapesPerLayer * nbRings + nbTriangleShapesPerLayer*(nbRings - 1));
        int nbTrianglePointsBottom = 3*nbPointsPerRing * 2; //two bottoms
        int nbTrianglePointsTop = 3*nbTriangleShapesPerLayer;
        int nbTotalTrianglePoints = nbTrianglePointsBottom + nbTrianglePointsSides + nbTrianglePointsTop;
        triangles = new int[nbTotalTrianglePoints];
        #region outside
        for (int i = 0; i < (nbRings - 1); i++)
        {
            for (int j = 0; j < nbPointsPerRing; j++)
            {
                triangles[nextStartingIndex + nbTriangleShapesPerLayer * 3 * i + 6 * j] = i * nbPointsPerRing + j;
                triangles[nextStartingIndex + nbTriangleShapesPerLayer * i * 3 + 6 * j + 1] = i * nbPointsPerRing + (j + 1) % nbPointsPerRing;
                triangles[nextStartingIndex + nbTriangleShapesPerLayer * i * 3 + 6 * j + 2] = i * nbPointsPerRing + j + nbPointsPerRing;//next row up

                triangles[nextStartingIndex + nbTriangleShapesPerLayer * i * 3 + 6 * j + 3] = i * nbPointsPerRing + (j + 1) % nbPointsPerRing;
                triangles[nextStartingIndex + nbTriangleShapesPerLayer * i * 3 + 6 * j + 4] = i * nbPointsPerRing + (j + 1) % nbPointsPerRing + nbPointsPerRing; //next row up
                triangles[nextStartingIndex + nbTriangleShapesPerLayer * i * 3 + 6 * j + 5] = i * nbPointsPerRing + j + nbPointsPerRing; //next row up
            }
        }
        nextStartingIndex += (nbRings - 1) * nbPointsPerRing * 6;
        #endregion
        #region inside
        for (int i = 1; i < (nbRings - 1); i++)
        {
            for (int j = 0; j < nbPointsPerRing; j++)
            {
                triangles[nextStartingIndex + nbTriangleShapesPerLayer * 3 * (i - 1) + 6 * j] = (i - 1) * nbPointsPerRing + j + offsetToInside;
                triangles[nextStartingIndex + nbTriangleShapesPerLayer * (i - 1) * 3 + 6 * j + 1] = (i - 1) * nbPointsPerRing + j + nbPointsPerRing + offsetToInside;//next row up
                triangles[nextStartingIndex + nbTriangleShapesPerLayer * (i - 1) * 3 + 6 * j + 2] = (i - 1) * nbPointsPerRing + (j + 1) % nbPointsPerRing + offsetToInside;

                triangles[nextStartingIndex + nbTriangleShapesPerLayer * (i - 1) * 3 + 6 * j + 3] = (i - 1) * nbPointsPerRing + (j + 1) % nbPointsPerRing + offsetToInside;
                triangles[nextStartingIndex + nbTriangleShapesPerLayer * (i - 1) * 3 + 6 * j + 4] = (i - 1) * nbPointsPerRing + j + nbPointsPerRing + offsetToInside; //next row up
                triangles[nextStartingIndex + nbTriangleShapesPerLayer * (i - 1) * 3 + 6 * j + 5] = (i - 1) * nbPointsPerRing + (j + 1) % nbPointsPerRing + nbPointsPerRing + offsetToInside; //next row up
            }
        }
        nextStartingIndex += (nbRings - 2) * nbPointsPerRing * 6;
        #endregion
        #region bottoms
        Vector3 zeroVector = new Vector3(0, 0, 0);
        for (int i = 0; i < nbPointsPerRing; i++)
        {
            triangles[nextStartingIndex + 3 * i] = offsetToBottom1 + i;
            triangles[nextStartingIndex + 3 * i + 1] = offsetToBottom2 - 1;
            triangles[nextStartingIndex + 3 * i + 2] = offsetToBottom1 + (i + 1) % nbPointsPerRing;
        }
        nextStartingIndex += nbPointsPerRing * 3;
        for (int i = 0; i < nbPointsPerRing; i++)
        {
            triangles[nextStartingIndex + 3 * i] = offsetToBottom2 + i;
            triangles[nextStartingIndex + 3 * i + 1] = offsetToBottom2 + (i + 1) % nbPointsPerRing;
            triangles[nextStartingIndex + 3 * i + 2] = offsetToTop1 - 1;
        }
        nextStartingIndex += nbPointsPerRing * 3;
        #endregion
        #region tops
        for (int j = 0; j < nbPointsPerRing; j++)
        {
            triangles[nextStartingIndex + 6 * j] = offsetToTop1 + j;
            //Debug.Log("index for triangles is " + (nextStartingIndex + 6 * j) + " for vertex is " + (offsetToTop1 + j) + " and the vertex vector is " + vertices[offsetToTop1 + j]);
            triangles[nextStartingIndex + 6*j+1] = offsetToTop1 + (j + 1) % nbPointsPerRing;
            //Debug.Log("index for triangles is " + (nextStartingIndex + 6 * j +1) + " for vertex is " + (offsetToTop1 + j+nbPointsPerRing) + " and the vertex vector is " + vertices[offsetToTop1 + j + nbPointsPerRing]);
            triangles[nextStartingIndex + 6*j+2] =  offsetToTop1 + j + nbPointsPerRing;//inner
            //Debug.Log("index for triangles is " + (nextStartingIndex + 6 * j +2) + " for vertex is " + (offsetToTop1 + (j + 1) % nbPointsPerRing) + " and the vertex vector is " + vertices[offsetToTop1 + (j + 1) % nbPointsPerRing]);

            triangles[nextStartingIndex + 6*j+3] = offsetToTop1 + (j + 1) % nbPointsPerRing;
            //Debug.Log("index for triangles is " + (nextStartingIndex + 6 * j +3) + " for vertex is " + (offsetToTop1 + (j + 1) % nbPointsPerRing) + " and the vertex vector is " + vertices[offsetToTop1 + (j + 1) % nbPointsPerRing]);
            triangles[nextStartingIndex + 6*j+4] = offsetToTop1 + nbPointsPerRing + (j + 1) % nbPointsPerRing;//inner
            //Debug.Log("index for triangles is " + (nextStartingIndex + 6 * j +4) + " for vertex is " + (offsetToTop1 + j + nbPointsPerRing) + " and the vertex vector is " + vertices[offsetToTop1 + j + nbPointsPerRing]);
            triangles[nextStartingIndex + 6*j+5] =  offsetToTop1 + j + nbPointsPerRing;//inner
            //Debug.Log("index for triangles is " + (nextStartingIndex + 6 * j + 5) + " for vertex is " + (offsetToTop1 + nbPointsPerRing + (j + 1) % nbPointsPerRing) + " and the vertex vector is " + vertices[offsetToTop1 + nbPointsPerRing + (j + 1) % nbPointsPerRing]);
        }
        #endregion
        mesh.vertices = vertices;
        //mesh.normals = normales;
        //mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.Optimize();
        MeshCollider meshc = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
        meshc.sharedMesh = mesh; // Give it your mesh here.
        #endregion

        #region interactionSetup
        percentAffectRings = new float[nbRings];
        maxVertexDistanceFromCenter = CalculateNewMaxDistance();
        #endregion 
    }
    void Update()
    {
        if (isInteracting) //isInteracting will be set by some sort of bottle manager
        {
            //Debug.Log("alright we're interacting!");  
            Vector3 handPosition = interactingController.transform.position; //CHANGE to the actual position of the hand. Figure this out later, i just want to get a skeleton written
            Vector3 localHandPosition = transform.InverseTransformPoint(handPosition);
            float dz = localHandPosition.z - vertices[vertexInteractingWith].z;
            float dr = new Vector2(localHandPosition.x, localHandPosition.y).magnitude - new Vector2(vertices[vertexInteractingWith].x, vertices[vertexInteractingWith].y).magnitude;
            float dtheta = (Mathf.Atan2(localHandPosition.y, localHandPosition.x) - Mathf.Atan2(vertices[vertexInteractingWith].y, vertices[vertexInteractingWith].x)) % _2pi;
            for (int i=0; i < nbRings; i++) //yes we're copy-pasting code. get over it. It's confusing enough to write this, we don't need you criticizing it
            {
                if (i==0) //this is the first row. We need to move the bottom as well and no inner points
                {                   
                    for (int j=0; j < nbPointsPerRing; j++)
                    {
                        float newRadius = new Vector2(vertices[j].x, vertices[j].y).magnitude + dr*percentAffectRings[i];
                        float newTheta = (Mathf.Atan2(vertices[j].y, vertices[j].x) + dtheta*percentAffectRings[i]) % _2pi;
                        float newZ = vertices[j].z + dz*percentAffectRings[i];
                        vertices[j] = new Vector3(newRadius * Mathf.Cos(newTheta), newRadius * Mathf.Sin(newTheta), newZ);
                        vertices[j + offsetToBottom1] = vertices[j]; //set the bottom one too
                    }
                }
                if (i == 1) //this is the second row. We need to move the other bottom as well as inner points
                {
                    for (int j = 0; j < nbPointsPerRing; j++)
                    {
                        float newRadius = new Vector2(vertices[j+i*nbPointsPerRing].x, vertices[j+i*nbPointsPerRing].y).magnitude + dr * percentAffectRings[i];
                        float newTheta = (Mathf.Atan2(vertices[j + i * nbPointsPerRing].y, vertices[j + i * nbPointsPerRing].x) + dtheta * percentAffectRings[i]) % _2pi;
                        float newZ = vertices[j + i * nbPointsPerRing].z + dz * percentAffectRings[i];
                        vertices[j + i * nbPointsPerRing] = new Vector3(newRadius * Mathf.Cos(newTheta), newRadius * Mathf.Sin(newTheta), newZ);
                        vertices[j + offsetToBottom2] = vertices[j + i * nbPointsPerRing]; //set the bottom one too 
                        float newInnerRadius = newRadius - (outerRadius - innerRadius);
                        vertices[j + offsetToInside] = new Vector3(newInnerRadius * Mathf.Cos(newTheta), newInnerRadius * Mathf.Sin(newTheta), newZ);
                    }
                }
                if(i==(nbRings-2)) //this is the second-to-last row. We need to make the difference in inner radius be that for the neck, not the bottle
                {
                    for (int j = 0; j < nbPointsPerRing; j++)
                    {
                        float newRadius = new Vector2(vertices[j + i * nbPointsPerRing].x, vertices[j + i * nbPointsPerRing].y).magnitude + dr * percentAffectRings[i];
                        float newTheta = (Mathf.Atan2(vertices[j + i * nbPointsPerRing].y, vertices[j + i * nbPointsPerRing].x) + dtheta * percentAffectRings[i]) % _2pi;
                        float newZ = vertices[j + i * nbPointsPerRing].z + dz * percentAffectRings[i];
                        vertices[j + i * nbPointsPerRing] = new Vector3(newRadius * Mathf.Cos(newTheta), newRadius * Mathf.Sin(newTheta), newZ);
                        float newInnerRadius = newRadius - (outerRadiusNeck - innerRadiusNeck);
                        vertices[j + offsetToInside] = new Vector3(newInnerRadius * Mathf.Cos(newTheta), newInnerRadius * Mathf.Sin(newTheta), newZ);
                    }
                }
                if(i==(nbRings-1)) //this is the last row. We need to move the top as well as inner points
                {
                    for (int j = 0; j < nbPointsPerRing; j++)
                    {
                        float newRadius = new Vector2(vertices[j + i * nbPointsPerRing].x, vertices[j + i * nbPointsPerRing].y).magnitude + dr * percentAffectRings[i];
                        float newTheta = (Mathf.Atan2(vertices[j + i * nbPointsPerRing].y, vertices[j + i * nbPointsPerRing].x) + dtheta * percentAffectRings[i]) % _2pi;
                        float newZ = vertices[j + i * nbPointsPerRing].z + dz * percentAffectRings[i];
                        vertices[j + i * nbPointsPerRing] = new Vector3(newRadius * Mathf.Cos(newTheta), newRadius * Mathf.Sin(newTheta), newZ);
                        vertices[j + offsetToTop1] = vertices[j + i * nbPointsPerRing]; //set the top outer
                        float newInnerRadius = newRadius - (outerRadiusNeck - innerRadiusNeck);
                        vertices[j + offsetToInside] = new Vector3(newInnerRadius * Mathf.Cos(newTheta), newInnerRadius * Mathf.Sin(newTheta), newZ);
                        vertices[j + offsetToTop2] = vertices[j + offsetToInside];
                    }
                }
                else //this is any nice row in the middle. We need to move inner points
                {
                    for (int j = 0; j < nbPointsPerRing; j++)
                    {
                        float newRadius = new Vector2(vertices[j + i * nbPointsPerRing].x, vertices[j + i * nbPointsPerRing].y).magnitude + dr * percentAffectRings[i];
                        float newTheta = (Mathf.Atan2(vertices[j + i * nbPointsPerRing].y, vertices[j + i * nbPointsPerRing].x) + dtheta * percentAffectRings[i]) % _2pi;
                        float newZ = vertices[j + i * nbPointsPerRing].z + dz * percentAffectRings[i];
                        vertices[j + i * nbPointsPerRing] = new Vector3(newRadius * Mathf.Cos(newTheta), newRadius * Mathf.Sin(newTheta), newZ);
                        float newInnerRadius = newRadius - (outerRadius - innerRadius);
                        vertices[j + offsetToInside] = new Vector3(newInnerRadius * Mathf.Cos(newTheta), newInnerRadius * Mathf.Sin(newTheta), newZ);
                    }
                }
            }
            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

        }
    }

    public void AttemptInteraction(GameObject controller)
    {
        Debug.Log("in attempt interaction");
        if(interactingController == null)
        {
            Debug.Log("there is no interacting controller yet");
            Vector3 handPosition = controller.transform.position; //CHANGE to the actual position of the hand. Figure this out later, i just want to get a skeleton written
            int closestVertex = FindClosestValidVertex(handPosition);
            if (closestVertex >= 0)
            {
                Debug.Log("you're close enough to have found a vertex!");
                vertexInteractingWith = closestVertex;
                StartInteraction(controller);
            }
            else
            {
                Debug.Log("not close enough to have found a vertex");
            }
        }       
    }

    void StartInteraction(GameObject controller) //this will change once we figure out how we're doing events with the controller
    {
        isInteracting = true;
        Debug.Log("isinteracting is" + isInteracting);
        interactingController = controller;
        controller.GetComponent<VRTK_ControllerEvents>().AliasReshapeOff += new VRTK.ControllerInteractionEventHandler(EndInteraction); //subscribe to that controller - when you let go of the button, ends the interaction
        int rowNumber = vertexInteractingWith / nbPointsPerRing;
        Debug.Log("row number we clicked is " + rowNumber + " out of " + nbRings);
        for (int i = 0; i < percentAffectRings.Length; i++)
        {
            if(i < rowNumber)
            {
                percentAffectRings[i] = (float)i / (float)rowNumber; 
            }
            else
            {
                percentAffectRings[i] = 1.0f - (float)(i - rowNumber) / (float)(nbRings-rowNumber);
            }
            Debug.Log("percentAffectRings[" + i + "] is " + percentAffectRings[i]);
        }
        
    }

    public void EndInteraction(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        isInteracting = false;
        interactingController = null;
        maxVertexDistanceFromCenter = CalculateNewMaxDistance(); //stuff will have moved around
    }

    float CalculateNewMaxDistance()
    {
        float maxDist = 0;
        for (int i=0; i < vertices.Length; i++)
        {
            float currentDist = vertices[i].magnitude;
            if (currentDist > maxDist)
            {
                maxDist = currentDist;
            }
        }
        return maxDist;
    }

    int FindClosestValidVertex(Vector3 handPosition) 
    {
        int closestVertexInRange = -1;
        Vector3 localHandPosition = transform.InverseTransformPoint(handPosition);
        Debug.Log("local hand position is" + localHandPosition);
        Debug.Log("maxVertexDistFromCenter is " + maxVertexDistanceFromCenter);
        if (localHandPosition.magnitude <= maxVertexDistanceFromCenter) //if the hand is at least closer than the farthest vertex
        {
            Debug.Log("We're at least closer than the farthest vertex");
            float closestDistanceSoFar = float.PositiveInfinity;
            for (int i = 0; i < offsetToInside; i++) //we just want to loop through the vertices on the outside
            {
                float currentDistance = Vector3.Distance(vertices[i], localHandPosition);
                if (currentDistance < distanceThreshold && currentDistance < closestDistanceSoFar)
                {
                    Debug.Log("Someone is the new closest vertex!!");
                    closestVertexInRange = i;
                    closestDistanceSoFar = currentDistance;
                }
            }
        }
        return closestVertexInRange; //will return -1 if there's no vertex
    }
    
}
