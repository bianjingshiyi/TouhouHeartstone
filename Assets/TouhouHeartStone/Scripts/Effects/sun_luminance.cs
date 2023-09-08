using UnityEngine;
using System.Collections;

public class sun_luminance : MonoBehaviour
{
	//An array of Objects that stores the results of the Resources.LoadAll() method
	private Object[] objects;
	//Each returned object is converted to a Texture and stored in this array
	private Texture[] textures;
	//With this Material object, a reference to the game object Material can be stored
	private Material goMaterial;
	//An integer to advance frames
	private float speed=10f;
	private float time=0f;
	private int prev_time=0;
	
	void Awake()
	{
		//Get a reference to the Material of the game object this script is attached to
		//this.goMaterial = this.renderer.material;
			}

	void Start ()
	{
		//Load all textures found on the Sequence folder, that is placed inside the resources folder
		this.objects = Resources.LoadAll("luma", typeof(Texture));

		//Initialize the array of textures with the same size as the objects array
		this.textures = new Texture[objects.Length];

		//Cast each Object to Texture and store the result inside the Textures array
		for(int i=0; i < objects.Length;i++)
		{
			this.textures[i] = (Texture)this.objects[i];
		}
	}

	void Update ()
	{
		time+=Time.deltaTime*speed;
		if (prev_time!= Mathf.Round(time)){
			prev_time=(int)Mathf.Round(time);
			if (prev_time>=objects.Length){
				prev_time=0;
				time=0f;
			}
			 this.GetComponent<Renderer>().material.SetTexture("_Illum",this.textures[prev_time]);
		//this.renderer.material.= this.textures[prev_time];
		print (this.GetComponent<Renderer>().material.GetTexture("_Illum"));
		}
    } 

}