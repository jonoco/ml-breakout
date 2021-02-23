using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBlockCreator : MonoBehaviour
{
    // to indicate block in current scene (in Inspector)
    // and parent block, which will always be "Blocks" empty object
    [SerializeField] GameObject blockObjType;

    // our project uses fixed 4-3 ratio, so these world units will not be changing
    [SerializeField] float screenWidthWorld = 16f;
    // float screenHeightWorld = 12f; // not used, but keeping just in case

    // for random block settings - max and min widths
    [SerializeField] int minBlockWidthWorld = 1;
    [SerializeField] int maxBlockWidthWorld = 4;

    // for random block settings - start row to place blocks
    [Range(2,6)]
    [SerializeField] float blockStartRow = 5f;

    // starting block width
    [SerializeField] int prefabBlockWidthWorld = 1;

    [SerializeField] float spaceBetweenBlocks = 0.1f;
    [SerializeField] float blockHeightScale = 0.5f;

    private bool isBlockFR(){
        return blockObjType.name == "block_fr";
    }

    public void setupBlocks()
    {
        // Used below when creating blocks to create them under the "Blocks"
        // empty game object for optimal organization

        // destroy all blocks that are initially on the board (i.e., testing blocks)
        DestroyExistingBlocks();
        // re-populate the board with blocks in a predetermined way
        placeBlocks();
    }

    // Frannie's Level - others can use if it's helpful
    private void DestroyExistingBlocks()
    {
        foreach(Transform child in transform)
        {
            if (child.gameObject.GetComponent<Block>())
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
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
        GameObject newBlock = Instantiate(blockObjType, 
                    transform, // need this to it groups under "Blocks"
                    false
        );
        newBlock.transform.localPosition = new Vector2(blockMidPoint, blockRow);
        return newBlock;
    }

    // Frannie's Level - dynamically and randomly setting up blocks
    // each block's width (column space) is what will be random - always will be 5 rows
    // block vector position is the MIDDLE OF THE BLOCK
    public void placeBlocks()
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
            blockRow += (blockHeightScale + spaceBetweenBlocks);

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
                var newBlock = createBlock(blockMidpoint, blockRow);
                
                // update block scale based on block width, note scale is a PERCENTAGE
                scaleChange = new Vector2(blockWidth/prefabBlockWidthWorld, blockHeightScale);
                newBlock.transform.localScale = scaleChange;

                // Update counter variables
                rowWidth += widths[1]; // total widths in position 1
                blocksPlaced += 1;    
            }// while loop
        } // for loop
    } // end function

}
