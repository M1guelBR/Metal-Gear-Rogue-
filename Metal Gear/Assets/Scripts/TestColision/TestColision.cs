using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestColision : MonoBehaviour
{

    public List<BoxCollider> cubosDetectar;
    Material mat;
    public bool colisiona;
    public Color colorLibre; public Color colorColision;
    List<Vector3> puntosColision = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        mat = this.GetComponent<MeshRenderer>().materials[0];
    }

    // Update is called once per frame
    void Update()
    {
        puntosColision = new List<Vector3>();
        colisiona = SolapanSalas();//DetectaColision();

        mat.color = colisiona ? colorColision : colorLibre;
    }

    void DetectaColision()
    {
        /*
        colisiona = false;
        foreach(BoxCollider cubo in cubosDetectar)
        {
            if (CuboEnCubo(this.GetComponent<BoxCollider>(), cubo))
            {
                colisiona = true;
                break;
            }
            if (CuboEnCubo(cubo, this.GetComponent<BoxCollider>()))
            {
                colisiona = true;
                break;
            }
        }
        */
    }

    bool SolapanSalas()
    {
        //List<GameObject> aCheck = new List<GameObject>(); aCheck.AddRange(salasRef);
        //aCheck.Add(salaBack); if (encajeBack != null) { aCheck.Add(encajeBack); }

        for (int i = 0; i < cubosDetectar.Count - 1; i++)
        {
            for (int j = i + 1; j < cubosDetectar.Count; j++)
            {
                //print("sala A : " + cubosDetectar[i].name + ", sala B : " + cubosDetectar[j].name);

                if (DetectaColisionSalas(cubosDetectar[i], cubosDetectar[j]))
                    return true;
            }
        }
        return false;
    }

    class Face
    {
        public Vector3 dir1;
        public Vector3 dir2;

        public float anchor1;
        public float anchor2;

        public Vector3 normal;
        public Vector3 point;
    }

    bool DetectaColisionSalas(BoxCollider cajaA, BoxCollider cajaB)
    {
        Vector3 C1 = cajaA.transform.position;

        foreach (Face face in Faces(cajaB))
        {
            Vector3 d1 = face.dir1; Vector3 d2 = face.dir2;
            //These are the vectors that "construct" the plane the face is in
            //Can be obtained applying the box rotation to the standard base vectors

            float anchor1 = face.anchor1; float anchor2 = face.anchor2;

            Vector3 n = face.normal; //It has to be normalized, obtained by cross product

            Vector3 point = face.point;

            //puntosColision.Add(point);

            //This is the point that lies exactly at the center
            //of the face. Can be easily obtained given the center and the dimensions
            foreach (Face faceA in Faces(cajaA))
            {
                Vector3 translation = point - faceA.point;

                translation = Vector3.ProjectOnPlane(translation, n);


                float distance1 = Mathf.Abs(Vector3.Dot(translation, d1));
                float distance2 = Mathf.Abs(Vector3.Dot(translation, d2));
                if (distance1 > anchor1 || distance2 > anchor2)
                    continue;

                Vector3 nearestPoint = point - translation;

                if (PuntoEnCaja(nearestPoint, cajaA))
                {
                    puntosColision.Add(nearestPoint);

                    return true;
                }
            }


        }
        return false;
    }

    Face[] Faces(BoxCollider caja)
    {
        //Sacamos todas las caras
        Face face0 = new Face(); Face face1 = new Face(); Face face2 = new Face();
        Face face3 = new Face(); Face face4 = new Face(); Face face5 = new Face();
        Face[] faces = new Face[6] { face0, face1, face2, face3, face4, face5};

        //0 la de arriba
        faces[0].dir1 = caja.transform.forward; faces[0].dir2 = caja.transform.right;
        faces[0].anchor1 = caja.transform.localScale.z/2; faces[0].anchor2 = caja.transform.localScale.x/2;
        faces[0].normal = caja.transform.up;
        faces[0].point = caja.transform.position + (caja.transform.up * caja.transform.localScale.y/2);

        //1 la de abajo
        faces[1].dir1 = caja.transform.right; faces[1].dir2 = caja.transform.up;
        faces[1].anchor1 = caja.transform.localScale.x/2; faces[1].anchor2 = caja.transform.localScale.z /2;
        faces[1].normal = -caja.transform.up;
        faces[1].point = caja.transform.position - (caja.transform.up * caja.transform.localScale.y/2);

        //2 der
        faces[2].dir1 = caja.transform.up; faces[2].dir2 = caja.transform.forward;
        faces[2].anchor1 = caja.transform.localScale.y/2; faces[2].anchor2 = caja.transform.localScale.z/2;
        faces[2].normal = caja.transform.right;
        faces[2].point = caja.transform.position + (caja.transform.right * caja.transform.localScale.x/2);

        //3 izq
        faces[3].dir1 = caja.transform.forward; faces[3].dir2 = caja.transform.up;
        faces[3].anchor1 = caja.transform.localScale.z/2; faces[3].anchor2 = caja.transform.localScale.y/2;
        faces[3].normal = -caja.transform.right;
        faces[3].point = caja.transform.position - (caja.transform.right * caja.transform.localScale.x/2);

        //4 alante
        faces[4].dir1 = caja.transform.right; faces[4].dir2 = caja.transform.up;
        faces[4].anchor1 = caja.transform.localScale.x/2; faces[4].anchor2 = caja.transform.localScale.y/2;
        faces[4].normal = caja.transform.forward;
        faces[4].point = caja.transform.position + (caja.transform.forward * caja.transform.localScale.z/2);

        //5 detras
        faces[5].dir1 = caja.transform.up; faces[5].dir2 = caja.transform.forward;
        faces[5].anchor1 = caja.transform.localScale.y/2; faces[5].anchor2 = caja.transform.localScale.z/2;
        faces[5].normal = -caja.transform.forward;
        faces[5].point = caja.transform.position - (caja.transform.forward * caja.transform.localScale.z/2);



        return faces;
    }


    private void OnDrawGizmosSelected()
    {
        foreach (Vector3 punto in puntosColision)
            Gizmos.DrawSphere(punto, 0.1f);
    }

    bool PuntoEnCaja(Vector3 punto, BoxCollider caja)
    {
        float offset = 0.01f;
        Vector3 translation = punto - caja.transform.position;

        bool x = Mathf.Abs(Vector3.Dot(caja.transform.right, translation)) <= (Mathf.Abs(caja.transform.localScale.x / 2) + offset);
        bool y = Mathf.Abs(Vector3.Dot(caja.transform.up, translation)) <= (Mathf.Abs(caja.transform.localScale.y / 2) + offset);
        bool z = Mathf.Abs(Vector3.Dot(caja.transform.forward, translation)) <= (Mathf.Abs(caja.transform.localScale.z / 2) + offset);

        return (x && y && z);
    }

}
