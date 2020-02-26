using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PMBShader : MonoBehaviour
{
    public ComputeShader shaderRef;
    public ComputeShader testShader;

    public void Awake()
    {
        int kernel1 = testShader.FindKernel("Test1");
        int kernel2 = testShader.FindKernel("Test2");
        int kernel3 = testShader.FindKernel("Test3");
        int kernel4 = testShader.FindKernel("Test4");
        int kernel5 = testShader.FindKernel("Test5");

        int[] args = new int[4];
        ComputeBuffer buffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        buffer.SetData(new int[] { 0,0,0,0});

        testShader.SetBuffer(kernel1, "buffer1", buffer);
        testShader.Dispatch(kernel1, 1, 1, 1);

        buffer.GetData(args);
        if (args[0] != 2)
        {
            Debug.LogError("testShader1 failed expected 2 but got: " + args[0]);
        }

        buffer.Release();

        buffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        buffer.SetData(new int[] { 0, 0, 0, 0 });

        testShader.SetBuffer(kernel2, "buffer2", buffer);
        testShader.Dispatch(kernel2, 1, 1, 1);

        buffer.GetData(args);
        if (args[0] != 2)
        {
            Debug.LogError("testShader2 failed expected 2 but got: " + args[0]);
        }

        buffer.Release();

        buffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        buffer.SetData(new int[] { 0, 0, 0, 0 });

        testShader.SetBuffer(kernel3, "buffer3", buffer);
        testShader.Dispatch(kernel3, 1, 1, 1);

        buffer.GetData(args);
        if (args[0] != 5)
        {
            Debug.LogError("testShader3 failed expected 5 but got: " + args[0]);
        }

        buffer.Release();

        buffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        buffer.SetData(new int[] { 0, 0, 0, 0 });

        testShader.SetBuffer(kernel4, "buffer4", buffer);
        testShader.Dispatch(kernel4, 1, 1, 1);

        buffer.GetData(args);
        if (args[0] != 2)
        {
            Debug.LogError("testShader4 failed expected 2 but got: " + args[0]);
        }

        buffer.Release();

        buffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        buffer.SetData(new int[] { 0, 0, 0, 0 });

        testShader.SetBuffer(kernel5, "buffer5", buffer);
        testShader.Dispatch(kernel5, 1, 1, 1);

        buffer.GetData(args);
        if (args[0] != 2)
        {
            Debug.LogError("testShader5 failed expected 2 but got: " + args[0]);
        }

        buffer.Release();
    }
}
