using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float speed = 20;
    public float rotSpeed = 10;
    
    Rigidbody rBody;
    private Transform eye;
 

	void Start () {
        rBody = GetComponent<Rigidbody>();
	}

    
    void FixedUpdate () {

        float rot = Input.GetAxis("Horizontal");
        float move = Input.GetAxis("Vertical");

        float rotY = rBody.rotation.eulerAngles.y;
        rotY += rotSpeed * rot;
        rBody.MoveRotation(Quaternion.Euler(0, rotY, 0));

        rBody.velocity = transform.forward * speed * move;
        //rigidbody.AddForce(transform.forward * speed * move);
        //Vector3 pos = rigidbody.position;
        //pos += transform.forward * move * speed;
        //rigidbody.MovePosition(pos);
	
    }
}
