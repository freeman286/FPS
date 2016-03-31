using UnityEngine;
using UnityEngine.Networking;

public class SpawnBlocks : NetworkBehaviour {

    public GameObject[] blocks;

    private int maxBlocks;
    private int currentBlocks;
    private int x;
    private int z;
    private int rot;


    void Start() {
        GenerateBlocks();
    }


    void GenerateBlocks()  {
        Random.seed = System.DateTime.Now.Day * System.DateTime.Now.Month * System.DateTime.Now.Year;
        
        maxBlocks = Random.Range(30, 50);
        while (currentBlocks <= maxBlocks) {
            GameObject _block = blocks[Random.Range(0, blocks.Length)];
            x = Random.Range(-28, 28);
            z = Random.Range(-28, 28);
            rot = Random.Range(0, 3) * 90;
            Collider[] _hitColliders = Physics.OverlapSphere(new Vector3(x, 3, z), _block.transform.localScale.x * 5);
            if (_hitColliders.Length < 2) {
                GameObject _block1 = (GameObject)Instantiate(_block, new Vector3(x, 3, z), Quaternion.Euler(0, rot, 0));
                GameObject _block2 = (GameObject)Instantiate(_block, new Vector3(-x, 3, -z), Quaternion.Euler(0, -rot, 0));

                _block1.transform.parent = gameObject.transform;
                _block2.transform.parent = gameObject.transform;

                currentBlocks += 1;
            }
        }
    }
}
