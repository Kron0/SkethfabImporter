using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UIElements;
using GLTF;
using UnityGLTF;

[Serializable]
public class Model
{
    public string uri;
    public string uid;
    public string name;
    public string username;
    public bool isDownloadable;
}
[Serializable]
public class ModelList
{
    public List<Model> models;
}
[Serializable]
public class ResultList
{
    public ModelList results;
}
[Serializable]
public class ModelDownloadInfo
{
    public gltfURL gltf;
}
[Serializable]
public class gltfURL
{
    public string url;
    public int size;
    public int expires;

}


public class SkethfabImportManager : MonoBehaviour
{
    [SerializeField]
    private GameObject DownloadButtonPrefab;

    [SerializeField]
    private GameObject SearchUI;

    [SerializeField]
    private GameObject DownloadUI;

    [SerializeField]
    private GameObject ContentHolder;

    [SerializeField]
    private Text SearchText;

    [SerializeField]
    private GameObject ErrorMessage;

    private const string SKETCHFAB_API_URL = "https://api.sketchfab.com/v3";
    private const string API_KEY = "37ffb72039e04578843dedf2166614a8";
    private UnityWebRequest request;

    private GLTFComponent gltfComponent;
    void Start()
    {
        SearchUI.SetActive(true);
        DownloadUI.SetActive(false);
        ErrorMessage.SetActive(false);
    }
    void Update()
    {
    }
    void Signin()
    {

    }
    public void Search()
    {
        ErrorMessage.SetActive(false);
        StopAllCoroutines();
        StartCoroutine(AsyncSearch());
    }
    private IEnumerator AsyncSearch()
    {
        Debug.Log("Searching: " + SearchText.text);
        UnityWebRequest uwr = UnityWebRequest.Get(SKETCHFAB_API_URL + "/search?q=" + SearchText.text);
        AsyncOperation request = uwr.SendWebRequest();

        while (!request.isDone)
        {
            //Debug.Log(request.progress);
            yield return null;
        }
        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log(uwr.error);
            ErrorMessage.SetActive(true);
        }
        else
        {
            SearchResponse(uwr.downloadHandler.text);
        }

    }
    void SearchResponse(string jsonResponse)
    {
        Debug.Log(jsonResponse);
        ResultList info = JsonUtility.FromJson<ResultList>(jsonResponse);
        PopulateList(info);
        SearchUI.SetActive(false);
        DownloadUI.SetActive(true);
    }

    void PopulateList(ResultList info)
    {
        foreach (Model m in info.results.models)
        {
            Debug.Log(m.name);
            if (m.isDownloadable)
            {
                GameObject button = Instantiate(DownloadButtonPrefab, Vector3.zero, Quaternion.identity, ContentHolder.transform);
                button.GetComponentInChildren<Text>().text = m.name + " | " + m.username;
                button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener( () =>
                {
                    StopAllCoroutines();
                    StartCoroutine(DownloadUID(m.uid));
                });
            }
        }
    }

    IEnumerator DownloadUID(string uid)
    {
        Debug.Log("Downloading: " + uid);
        UnityWebRequest uwr = UnityWebRequest.Get(SKETCHFAB_API_URL + "/models/" + uid + "/download");
        AsyncOperation request = uwr.SendWebRequest();
        while (!request.isDone)
        {
            //Debug.Log(request.progress);
            yield return null;
        }
        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            Download(uwr.downloadHandler.text);
        }
    }

    IEnumerator Download(string response)
    {
        Debug.Log(response);
        ModelDownloadInfo info = JsonUtility.FromJson<ModelDownloadInfo>(response);
        Debug.Log("Downloading Model");

        yield return Import(info.gltf.url);

        /*
        UnityWebRequest uwr = UnityWebRequest.Get(response);
        AsyncOperation request = uwr.SendWebRequest();
        while (!request.isDone)
        {
            Debug.Log(request.progress);
            yield return null;
        }
        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            Import(uwr.downloadHandler.text);
        }
        */
    }

    IEnumerator Import(string url)
    {
        Debug.Log("Importing Model");
        gltfComponent.GLTFUri = url;
        yield return gltfComponent.Load().AsCoroutine();

		//wait one frame for rendering to complete
		yield return null;
    }
}

