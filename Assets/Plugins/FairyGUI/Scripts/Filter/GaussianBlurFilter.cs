using FairyGUI;
using UnityEngine;

namespace FairyGUI
{
    public class GaussianBlurFilter : IFilter
    {
        //ref: https://blog.csdn.net/puppet_master/article/details/52783179

        public const float DefaultBlurRadius = 1.0f;
        public const int DefaultDownSample = 3;
        public const int DefaultIteration = 5;
        
        //模糊半径
        public float BlurRadius = DefaultBlurRadius;
        //降分辨率
        public int DownSample = DefaultDownSample;
        //迭代次数
        public int Iteration = DefaultIteration;


        private DisplayObject m_Target;
        private Material m_BlitMaterial;
        
        public DisplayObject target
        {
            get => m_Target;
            set
            {
                m_Target = value;
                m_Target.EnterPaintingMode(1, null);
                m_Target.onPaint += OnRenderImage;

                m_BlitMaterial = new Material(ShaderConfig.GetShader("FairyGUI/GaussianBlur"));
                m_BlitMaterial.hideFlags = DisplayObject.hideFlags;
            }
        }

        public void Update()
        {
            
        }

        public void Dispose()
        {
            m_Target.LeavePaintingMode(1);
            m_Target.onPaint -= OnRenderImage;
            m_Target = null;

            if (Application.isPlaying)
                Object.Destroy(m_BlitMaterial);
            else
                Object.DestroyImmediate(m_BlitMaterial);
        }

        public void OnRenderImage()
        {
            RenderTexture source = (RenderTexture)m_Target.paintingGraphics.texture.nativeTexture;
            RenderTexture rt1 = RenderTexture.GetTemporary(source.width >> DownSample, source.height >> DownSample, 0, source.format);
            RenderTexture rt2 = RenderTexture.GetTemporary(source.width >> DownSample, source.height >> DownSample, 0, source.format);
 
            //直接将原图拷贝到降分辨率的RT上
            Graphics.Blit(source, rt1);
 
            //进行迭代高斯模糊
            for(int i = 0; i < Iteration; i++)
            {
                //第一次高斯模糊，设置offsets，竖向模糊
                m_BlitMaterial.SetVector("_offsets", new Vector4(0, BlurRadius, 0, 0));
                Graphics.Blit(rt1, rt2, m_BlitMaterial);
                //第二次高斯模糊，设置offsets，横向模糊
                m_BlitMaterial.SetVector("_offsets", new Vector4(BlurRadius, 0, 0, 0));
                Graphics.Blit(rt2, rt1, m_BlitMaterial);
            }
 
            //将结果输出
            Graphics.Blit(rt1, source);
 
            //释放申请的两块RenderBuffer内容
            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
        }

        //todo:新增截屏组件，来处理相关逻辑，并能截取全部屏幕上的内容
        public static RenderTexture RenderOnce(Texture source)
        {
            RenderTexture rt = new RenderTexture(source.width, source.height, 0);
            Graphics.Blit(source, rt);
            
            RenderTexture rt1 = RenderTexture.GetTemporary(rt.width >> DefaultDownSample, rt.height >> DefaultDownSample, 0, rt.format);
            RenderTexture rt2 = RenderTexture.GetTemporary(rt.width >> DefaultDownSample, rt.height >> DefaultDownSample, 0, rt.format);
 
            //直接将原图拷贝到降分辨率的RT上
            Graphics.Blit(rt, rt1);
            
            var blitMaterial = new Material(ShaderConfig.GetShader("FairyGUI/GaussianBlur"));
            blitMaterial.hideFlags = DisplayObject.hideFlags;
 
            //进行迭代高斯模糊
            for(int i = 0; i < DefaultIteration; i++)
            {
                //第一次高斯模糊，设置offsets，竖向模糊
                blitMaterial.SetVector("_offsets", new Vector4(0, DefaultBlurRadius, 0, 0));
                Graphics.Blit(rt1, rt2, blitMaterial);
                //第二次高斯模糊，设置offsets，横向模糊
                blitMaterial.SetVector("_offsets", new Vector4(DefaultBlurRadius, 0, 0, 0));
                Graphics.Blit(rt2, rt1, blitMaterial);
            }
 
            //将结果输出
            Graphics.Blit(rt1, rt);
 
            //释放申请的两块RenderBuffer内容
            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
            
            Object.Destroy(blitMaterial);
            Object.Destroy(source);

            return rt;
        }
    }
}