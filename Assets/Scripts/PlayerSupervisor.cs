using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSupervisor : MonoBehaviour
{

    [SerializeField] GameManager gameManager;
    [SerializeField] int activeBlocks;

    // to indicate block in current scene (in Inspector)
    // and parent block, which will always be "Blocks" empty object
    [SerializeField] GameObject block;
    [SerializeField] GameObject parentBlock;

    // our project uses fixed 4-3 ratio, so these world units will not be changing
    [SerializeField] float screenWidthWorld = 16f;
    [SerializeField] float screenHeightWorld = 12f;

    // for random block settings - max and min widths
    [SerializeField] int minBlockWidthWorld = 1;
    [SerializeField] int maxBlockWidthWorld = 4;

    // for random block settings - start row to place blocks
    [SerializeField] float blockStartRow = 5f;

    // starting block width
    [SerializeField] int prefabBlockWidthWorld = 1;

    [SerializeField] float spaceBetweenBlocks = .05f;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        // Used below when creating blocks to create them under the "Blocks"
        // empty game object for optimal organization
        parentBlock = GameObject.Find("Blocks");

        // Frannie's level - setting up dynamically 
        if(block.name == "block_fr")
        {
            // destroy all blocks that are initially on the board (i.e., testing blocks)
            DestroyExistingBlocks();
            // re-populate the board with blocks in a predetermined way
            CreateStartingBlocks();
        }

        activeBlocks = GameObject.FindObjectsOfType<Block>().Length;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Frannie's Level - others can use if it's helpful
    public void DestroyExistingBlocks()
    {
        Block[] allBlocks = GameObject.FindObjectsOfType<Block>();
        foreach(Block block in allBlocks)
        {
            // if use just Destroy, object hangs around for awhile. do not use.
            DestroyImmediate(block.gameObject);  
        }
    }

    private float calculateBlockMidpoint(int blocksPlaced, float currBlockWidth, 
                                         float priorBlockWidth, float priorMidpoint)
    {
        float midpoint = 0;
        if(blocksPlaced == 0) 
        {
            midpoint = currBlockWidth/2 + spaceBetweenBlocks;
        } 
        else // all other blocks
        {
            midpoint = priorMidpoint +  priorBlockWidth/2 + 
                       currBlockWidth/2 + spaceBetweenBlocks;
        }
        return midpoint;
    }

    // Frannie - helper for dynamic block function
    private float[] calculateWidths(float randNum, float rowWidth)
    {
        float blockWidth = 0;
        float totalAddedWidth = 0;
        // if current randnum puts us outside of screen row width, 
        // take what we can that's left and assign that to width
        // putting buffers to avoid skinny blocks
        if(screenWidthWorld - rowWidth < maxBlockWidthWorld || 
           screenWidthWorld - rowWidth - randNum < 0)
        {
            blockWidth = screenWidthWorld - rowWidth - spaceBetweenBlocks*2;
            totalAddedWidth = blockWidth + spaceBetweenBlocks*2;
        } 
        else 
        {   
            blockWidth = randNum;  
            totalAddedWidth = blockWidth + spaceBetweenBlocks;        
        }
        float[] returnArr = new float[2];
        returnArr[0] = blockWidth;
        returnArr[1] = totalAddedWidth;
        return returnArr;
    }

    // Frannie - helper for dynamic block function
    private GameObject createBlock(float blockMidPoint, float blockRow)
    {
        // create new game object
        // x = midpoint position of block in screen space
        // y = current row position
        GameObject newBlock = Instantiate(block, 
                    new Vector2(blockMidPoint, blockRow), 
                    Quaternion.identity, 
                    parentBlock.transform // need this to it groups under "Blocks"
        );
        return newBlock;
    }

    // Frannie's Level - dynamically and randomly setting up blocks
    // each block's width (column space) is what will be random - always will be 5 rows
    // block vector position is the MIDDLE OF THE BLOCK
    public void CreateStartingBlocks()
    {
        float randNum;
        int blocksPlaced;
        float blockRow = blockStartRow;
        float blockMidpoint;
        float[] widths;
        float rowWidth;
        float blockWidth;
        float priorBlockWidth; 
        float priorBlockMidpoint; 
        Vector3 scaleChange;
        for(int i = 0; i < 5; i ++) // 5 rows
        {
            // curr row start position (i.e. y value for block)
            blockRow += (1f + spaceBetweenBlocks);

            // reset these values each iteration
            blocksPlaced = 0;  
            rowWidth = 0;
            blockMidpoint = 0;
            priorBlockWidth = 0;
            priorBlockMidpoint = 0;

            // While there is still space left in current row
            // ROW WIDTH INCLUDES SPACE BETWEEN BLOCKS AND BLOCK WIDTHS
            while(rowWidth < screenWidthWorld){

                randNum = UnityEngine.Random.Range(minBlockWidthWorld, maxBlockWidthWorld);
                widths = calculateWidths(randNum, rowWidth);
                blockWidth = widths[0]; // block widths in position 0
                blockMidpoint = calculateBlockMidpoint(blocksPlaced, blockWidth, 
                                                       priorBlockWidth, priorBlockMidpoint);
                priorBlockWidth = blockWidth;
                priorBlockMidpoint = blockMidpoint;
                GameObject newBlock = createBlock(blockMidpoint, blockRow);

                // update block scale based on block width, note scale is a PERCENTAGE
                scaleChange = new Vector2(blockWidth/prefabBlockWidthWorld - 1, 0);
                newBlock.transform.localScale += scaleChange;

                // Update counter variables
                rowWidth += widths[1]; // total widths in position 1
                blocksPlaced += 1;    
            }// while loop
        } // for loop
    } // end function





    public void BlockDestroyed()
    {
        --activeBlocks;
        if (activeBlocks <= 0)
        {
            gameManager.WinGame();
        }
    }
}
