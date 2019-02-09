using UnityEngine;

public class MeteorCollider : MonoBehaviour {

    public GameObject explosionPrefab;    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gem"))
        {
            Destroy(gameObject.transform.parent.gameObject);
            GameObject explosion = Instantiate(explosionPrefab);
            explosion.transform.position = other.transform.position;

            int x = other.gameObject.GetComponent<GemInfo>().xPos;
            int y = other.gameObject.GetComponent<GemInfo>().yPos;

            FindObjectOfType<GemManager>().DestroyGem(x, y);
        }
    }

    private void OnDestroy()
    {
        if (gameObject.transform.parent.name.Equals("lastMeteor"))
        {
            FindObjectOfType<GemManager>().isBonusActing = false;
        }
    }

}
