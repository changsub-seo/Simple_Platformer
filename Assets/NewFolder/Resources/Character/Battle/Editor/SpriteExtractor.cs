using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteExtractor
{
    // 마우스 우클릭 메뉴에 'Extract Sprite(s)' 라는 버튼을 만들어 줍니다.
    [MenuItem("Assets/Extract Sprite(s)")]
    public static void ExtractSprites()
    {
        // 선택한 모든 파일을 확인합니다.
        foreach (Object obj in Selection.objects)
        {
            if (obj is Sprite sprite)
            {
                ExtractSingleSprite(sprite);
            }
        }
    }

    private static void ExtractSingleSprite(Sprite sprite)
    {
        try
        {
            // 잘린 영역만큼의 새로운 빈 텍스처를 만듭니다.
            Texture2D newTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            
            // 원본에서 잘린 영역의 픽셀만 복사해 옵니다.
            Color[] pixels = sprite.texture.GetPixels(
                (int)sprite.rect.x, 
                (int)sprite.rect.y, 
                (int)sprite.rect.width, 
                (int)sprite.rect.height);
                
            newTexture.SetPixels(pixels);
            newTexture.Apply();

            // PNG 파일로 변환합니다.
            byte[] bytes = newTexture.EncodeToPNG();
            
            // 원본 파일이 있는 폴더 경로를 찾아서 그곳에 저장합니다.
            string path = AssetDatabase.GetAssetPath(sprite.texture);
            string directory = Path.GetDirectoryName(path);
            string newPath = Path.Combine(directory, sprite.name + ".png");

            File.WriteAllBytes(newPath, bytes);
            Debug.Log($"추출 완료! 저장 경로: {newPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"추출 실패! 원본 이미지의 Inspector에서 'Read/Write'가 체크되어 있는지 확인해주세요. 에러: {e.Message}");
        }
    }
}