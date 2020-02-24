using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using UnityEngine;

public class ThreadHelperTask
{
    public Action action;
    public bool completed = false;
    public bool canceled = false;

    public ThreadHelperTask(Action a)
    {
        this.action = a;
    }

    public void wait()
    {
        int i = 0;
        while (!completed)
        {
            Thread.Sleep(10);
            i++;
            if (i > 1000 || canceled)
            {
                Debug.LogWarning("canceled wait");
                return;
            }
        }
    }
}

public class MainThreadHelper : MonoBehaviour
{

    private static MainThreadHelper _instance = null;

    private ConcurrentQueue<ThreadHelperTask> tasks = new ConcurrentQueue<ThreadHelperTask>();

    public MainThreadHelper()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Debug.LogError("There should be only one instance");
        }
    }

    public void Awake()
    {
        Debug.Log("SystemInfo.batteryLevel=" + SystemInfo.batteryLevel
        + "\nSystemInfo.batteryStatus=" + SystemInfo.batteryStatus
        + "\nSystemInfo.copyTextureSupport=" + SystemInfo.copyTextureSupport
        + "\nSystemInfo.deviceModel=" + SystemInfo.deviceModel
        + "\nSystemInfo.deviceName=" + SystemInfo.deviceName
        + "\nSystemInfo.deviceType=" + SystemInfo.deviceType
        + "\nSystemInfo.deviceUniqueIdentifier=" + SystemInfo.deviceUniqueIdentifier
        + "\nSystemInfo.graphicsDeviceID=" + SystemInfo.graphicsDeviceID
        + "\nSystemInfo.graphicsDeviceName=" + SystemInfo.graphicsDeviceName
        + "\nSystemInfo.graphicsDeviceType=" + SystemInfo.graphicsDeviceType
        + "\nSystemInfo.graphicsDeviceVendor=" + SystemInfo.graphicsDeviceVendor
        + "\nSystemInfo.graphicsDeviceVendorID=" + SystemInfo.graphicsDeviceVendorID
        + "\nSystemInfo.graphicsDeviceVersion=" + SystemInfo.graphicsDeviceVersion
        + "\nSystemInfo.graphicsMemorySize=" + SystemInfo.graphicsMemorySize
        + "\nSystemInfo.graphicsMultiThreaded=" + SystemInfo.graphicsMultiThreaded
        + "\nSystemInfo.graphicsShaderLevel=" + SystemInfo.graphicsShaderLevel
        + "\nSystemInfo.graphicsUVStartsAtTop=" + SystemInfo.graphicsUVStartsAtTop
        + "\nSystemInfo.hasDynamicUniformArrayIndexingInFragmentShaders=" + SystemInfo.hasDynamicUniformArrayIndexingInFragmentShaders
        + "\nSystemInfo.hasHiddenSurfaceRemovalOnGPU=" + SystemInfo.hasHiddenSurfaceRemovalOnGPU
        + "\nSystemInfo.hasMipMaxLevel=" + SystemInfo.hasMipMaxLevel
        + "\nSystemInfo.maxComputeBufferInputsCompute=" + SystemInfo.maxComputeBufferInputsCompute
        + "\nSystemInfo.maxComputeBufferInputsDomain=" + SystemInfo.maxComputeBufferInputsDomain
        + "\nSystemInfo.maxComputeBufferInputsFragment=" + SystemInfo.maxComputeBufferInputsFragment
        + "\nSystemInfo.maxComputeBufferInputsGeometry=" + SystemInfo.maxComputeBufferInputsGeometry
        + "\nSystemInfo.maxComputeBufferInputsHull=" + SystemInfo.maxComputeBufferInputsHull
        + "\nSystemInfo.maxComputeBufferInputsVertex=" + SystemInfo.maxComputeBufferInputsVertex
        + "\nSystemInfo.maxComputeWorkGroupSize=" + SystemInfo.maxComputeWorkGroupSize
        + "\nSystemInfo.maxComputeWorkGroupSizeX=" + SystemInfo.maxComputeWorkGroupSizeX
        + "\nSystemInfo.maxComputeWorkGroupSizeY=" + SystemInfo.maxComputeWorkGroupSizeY
        + "\nSystemInfo.maxComputeWorkGroupSizeZ=" + SystemInfo.maxComputeWorkGroupSizeZ
        + "\nSystemInfo.maxCubemapSize=" + SystemInfo.maxCubemapSize
        + "\nSystemInfo.maxTextureSize=" + SystemInfo.maxTextureSize
        + "\nSystemInfo.minConstantBufferOffsetAlignment=" + SystemInfo.minConstantBufferOffsetAlignment
        + "\nSystemInfo.npotSupport=" + SystemInfo.npotSupport
        + "\nSystemInfo.operatingSystem=" + SystemInfo.operatingSystem
        + "\nSystemInfo.operatingSystemFamily=" + SystemInfo.operatingSystemFamily
        + "\nSystemInfo.processorCount=" + SystemInfo.processorCount
        + "\nSystemInfo.processorFrequency=" + SystemInfo.processorFrequency
        + "\nSystemInfo.processorType=" + SystemInfo.processorType
        + "\nSystemInfo.renderingThreadingMode=" + SystemInfo.renderingThreadingMode
        + "\nSystemInfo.supportedRandomWriteTargetCount=" + SystemInfo.supportedRandomWriteTargetCount
        + "\nSystemInfo.supportedRenderTargetCount=" + SystemInfo.supportedRenderTargetCount
        + "\nSystemInfo.supports2DArrayTextures=" + SystemInfo.supports2DArrayTextures
        + "\nSystemInfo.supports32bitsIndexBuffer=" + SystemInfo.supports32bitsIndexBuffer
        + "\nSystemInfo.supports3DRenderTextures=" + SystemInfo.supports3DRenderTextures
        + "\nSystemInfo.supports3DTextures=" + SystemInfo.supports3DTextures
        + "\nSystemInfo.supportsAccelerometer=" + SystemInfo.supportsAccelerometer
        + "\nSystemInfo.supportsAsyncCompute=" + SystemInfo.supportsAsyncCompute
        + "\nSystemInfo.supportsAsyncGPUReadback=" + SystemInfo.supportsAsyncGPUReadback
        + "\nSystemInfo.supportsAudio=" + SystemInfo.supportsAudio
        + "\nSystemInfo.supportsComputeShaders=" + SystemInfo.supportsComputeShaders
        + "\nSystemInfo.supportsCubemapArrayTextures=" + SystemInfo.supportsCubemapArrayTextures
        + "\nSystemInfo.supportsGeometryShaders=" + SystemInfo.supportsGeometryShaders
        + "\nSystemInfo.supportsGraphicsFence=" + SystemInfo.supportsGraphicsFence
        + "\nSystemInfo.supportsGyroscope=" + SystemInfo.supportsGyroscope
        + "\nSystemInfo.supportsHardwareQuadTopology=" + SystemInfo.supportsHardwareQuadTopology
        + "\nSystemInfo.supportsInstancing=" + SystemInfo.supportsInstancing
        + "\nSystemInfo.supportsLocationService=" + SystemInfo.supportsLocationService
        + "\nSystemInfo.supportsMipStreaming=" + SystemInfo.supportsMipStreaming
        + "\nSystemInfo.supportsMotionVectors=" + SystemInfo.supportsMotionVectors
        + "\nSystemInfo.supportsMultisampleAutoResolve=" + SystemInfo.supportsMultisampleAutoResolve
        + "\nSystemInfo.supportsMultisampledTextures=" + SystemInfo.supportsMultisampledTextures
        + "\nSystemInfo.supportsRawShadowDepthSampling=" + SystemInfo.supportsRawShadowDepthSampling
        + "\nSystemInfo.supportsRayTracing=" + SystemInfo.supportsRayTracing
        + "\nSystemInfo.supportsSeparatedRenderTargetsBlend=" + SystemInfo.supportsSeparatedRenderTargetsBlend
        + "\nSystemInfo.supportsSetConstantBuffer=" + SystemInfo.supportsSetConstantBuffer
        + "\nSystemInfo.supportsShadows=" + SystemInfo.supportsShadows
        + "\nSystemInfo.supportsSparseTextures=" + SystemInfo.supportsSparseTextures
        + "\nSystemInfo.supportsTessellationShaders=" + SystemInfo.supportsTessellationShaders
        + "\nSystemInfo.supportsTextureWrapMirrorOnce=" + SystemInfo.supportsTextureWrapMirrorOnce
        + "\nSystemInfo.supportsVibration=" + SystemInfo.supportsVibration
        + "\nSystemInfo.systemMemorySize=" + SystemInfo.systemMemorySize
        + "\nSystemInfo.unsupportedIdentifier=" + SystemInfo.unsupportedIdentifier
        + "\nSystemInfo.unsupportedIdentifier=" + SystemInfo.usesLoadStoreActions
        + "\nSystemInfo.usesReversedZBuffer=" + SystemInfo.usesReversedZBuffer);
    }

    public static MainThreadHelper instance()
    {
        if (_instance == null)
        {
            Debug.LogError("There should be an instance of MainThreadHelper in this Scene");
        }
        return _instance;
    }

    public ThreadHelperTask scheduleOnMainThread(Action action)
    {

        var task = new ThreadHelperTask(action);
        if (Thread.CurrentThread.ManagedThreadId == 1)
        {
            action();
            task.completed = true;
            return task;
        }
        tasks.Enqueue(task);
        return task;
    }

    public void cancelAllPendingTasks()
    {
        
        while (!tasks.IsEmpty)
        {
            ThreadHelperTask task;
            if (tasks.TryDequeue(out task))
            {
                Debug.Log("canceled Task");
                task.canceled = true;
            }
        }
    }

    public void Update()
    {
        ThreadHelperTask task;
        if (tasks.TryDequeue(out task))
        {
            if (!task.canceled)
            {
                task.action();
                task.completed = true;
            }
        }
    }
}