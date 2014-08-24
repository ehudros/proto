//Runtime terrain generator example

using UnityEngine;
using System.Collections;

public class Terrain2DGenerator : MonoBehaviour
{
    public Transform Target; //Player transform

    public Material Terrain2DMaterial; //Default terrain material
    public Material TerrainCapMaterial; //Default cap material

    private GameObject _lastTerrain2D; //last randomly generated terrain

    private float _lastTargetPos; //last Player position by X 


	void Start () 
    {
        CreateNextTerrain2D(Vector2.zero); //Create first terrain
	}

    void Update()
    {
        if (Target.transform.position.x > _lastTargetPos) //Is player already cross the line for creating new terrain?
        {
            _lastTargetPos += 50; //Change last Player position by x based on terrain width (50 by defaul)
            CreateNextTerrain2D(new Vector2(_lastTargetPos, 0));
        }
    }

    void FixedUpdate()
    {
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(Target.transform.position.x, Target.transform.position.y, Camera.main.transform.position.z), Time.deltaTime * 25f);
        Target.transform.Translate(Vector3.right * 0.5f);
    }

    void CreateNextTerrain2D(Vector2 position)
    {
        GameObject newTerrain = TerrainEditor2D.InstantiateTerrain2D(position); //Creating default terrain

        TerrainEditor2D myTerrain = newTerrain.GetComponent<TerrainEditor2D>();

        myTerrain.MainMaterial = Terrain2DMaterial;  //Assign material to generating terrain
        myTerrain.CapMaterial = TerrainCapMaterial;  //Assign material to terrain cap

        // --- Configure cap
        myTerrain.CapHeight = 0.75f;
        myTerrain.CapOffset = 0.1f;
        myTerrain.CreateCap = true; //Set TRUE if terrain cap will be generated

        if (_lastTerrain2D != null)
            myTerrain.RandomizeTerrain(_lastTerrain2D.GetComponent<TerrainEditor2D>().GetLastVertexPoint()); //Connect new terrain with latter
        else myTerrain.RandomizeTerrain();

        myTerrain.UpdateCollider(); //Calculate Edge Collider path for terrain

        //DONE! New terrain generated!
        //If you need to change terrain parameters (like: myTerrain.Width = 100; myTerrain.Height = 25; myTerrain.TextureSize = 50; etc.) use myTerrain.CreateTerrain(); before myTerrain.RandomizeTerrain();
        
        _lastTerrain2D = newTerrain;
    }
}
