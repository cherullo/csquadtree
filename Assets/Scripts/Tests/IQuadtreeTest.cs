using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

public class IQuadtreeTest : MonoBehaviour, IPointTest
{
    public TextAsset m_implementation;

    private IQuadtree<Component> m_tree;

    private float _sideLength;
    private Component[] _results;

    public void Initialize(float sideLength)
    {
        _sideLength = sideLength;

        string className = _GetTreeClassName (m_implementation) + "`1";
        UnityEngine.Debug.Log (className);

        System.Type t = System.Type.GetType( className );

        // TODO: Check type implements IQuadtree<Component>

        System.Type tg = t.MakeGenericType (typeof(Component));

        if (tg != null)
            m_tree = _CreateTestType (tg, sideLength);
    }

    public string GetName()
    {
        return m_implementation.name.Replace("`1", "") + "(" + _sideLength + ")";
    }

    protected string _GetTreeClassName(TextAsset p_treeFile)
    {
        string _namespace = null;
        string _classname = null;

        string[] lines = p_treeFile.text.Split ('\n');

        Regex namespaceRegex = new Regex (@"(namespace)\s+([\w\.]+)");
        Regex classnameRegex = new Regex (@"(class)\s+(\w+)");

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines [i];

            Match nsm = namespaceRegex.Match (line);
            if (nsm.Success == true)
            {
                _namespace = nsm.Groups[2].Captures [0].Value;
            }
            else
            {
                Match cnm = classnameRegex.Match (line);
                if (cnm.Success == true)
                {
                    _classname = cnm.Groups[2].Captures [0].Value;
                }
            }

            if ((_namespace != null) && (_classname != null))
                return _namespace + "." + _classname;
        }

        return null;
    }

    private IQuadtree<Component> _CreateTestType(System.Type p_type, float sideLength)
    {
        Dictionary<string, object> context = BuildConstructorContext (sideLength);

        return CreateInstance (p_type, context);
    }

    public RunResult RunTest (Vector2[] p_points, Component[] p_values, Vector2[] p_searches)
    {
        IRebuildableQuadtree<Component> rebuildableTree = m_tree as IRebuildableQuadtree<Component>;
        
        Component[] result;
        Stopwatch watch = new Stopwatch();

        watch.Start();

        if (rebuildableTree != null)
            result = RunTest (rebuildableTree, p_points, p_values, p_searches);
        else
            result = RunTest (m_tree, p_points, p_values, p_searches);

        watch.Stop();

        return new RunResult(result, watch.Elapsed.TotalMilliseconds);
    }

    private Component[] RunTest (IRebuildableQuadtree<Component> p_tree, Vector2[] p_points, Component[] p_values, Vector2[] p_searches)
    {
        if ((_results == null) || (_results.Length != p_searches.Length))
        {
            // First time
            _results = new Component[p_searches.Length];

            p_tree.Clear ();

            for (int i = 0; i < p_points.Length; i++)
            {
                p_tree.Add(p_points [i].x, p_points [i].y, p_values[i]);
            }
        }

        p_tree.Rebuild ();

        for (int i = 0; i < p_points.Length; i++)
        {
            _results [i] =  p_tree.ClosestTo (p_points [i].x, p_points [i].y).GetResult();
        }
        
        return _results;
    }

    private Component[] RunTest (IQuadtree<Component> p_tree, Vector2[] p_points, Component[] p_values, Vector2[] p_searches)
    {
        if ((_results == null) || (_results.Length != p_searches.Length))
            _results = new Component[p_searches.Length];

        p_tree.Clear ();

        for (int i = 0; i < p_points.Length; i++)
        {
            p_tree.Add(p_points [i].x, p_points [i].y, p_values[i]);
        }

        for (int i = 0; i < p_points.Length; i++)
        {
            _results [i] =  p_tree.ClosestTo (p_points [i].x, p_points [i].y).GetResult();
        }

        return _results;
    }

    protected Dictionary<string, object> BuildConstructorContext (float p_sideLength)
    {
        Dictionary<string, object> ret = new Dictionary<string, object> ();

        ret.Add ("p_bottomLeftX", 0.0f);
        ret.Add ("p_bottomLeftY", 0.0f);
        ret.Add ("p_topRightX", p_sideLength);
        ret.Add ("p_topRightY", p_sideLength);
        ret.Add ("p_sideLength", p_sideLength);

        return ret;
    }

    protected IQuadtree<Component> CreateInstance(System.Type p_classType, Dictionary<string, object> p_context)
    {
        ConstructorInfo constructor = FindConstructorMatchingContext (p_classType, p_context);

        if (constructor == null)
        {
            UnityEngine.Debug.LogError("Could not find a constructor for type '" + p_classType.Name + "' with parameter names contained in (" + DictionaryKeysToString(p_context) + ")");
        }

        return InvokeConstructor (constructor, p_context);
    }

    protected IQuadtree<Component> InvokeConstructor(ConstructorInfo p_constructor,  Dictionary<string, object> p_context)
    {
        return (IQuadtree<Component>) p_constructor.Invoke (GetParametersValues (p_constructor.GetParameters (), p_context));
    }

    protected object[] GetParametersValues( ParameterInfo[] p_parameters, Dictionary<string, object> p_context)
    {
        object[] ret = new object[p_parameters.Length];

        for(int i = 0; i < p_parameters.Length; i++)
        {
            ParameterInfo pi = p_parameters [i];

            ret [i] = p_context [pi.Name];
        }

        return ret;
    }

    protected ConstructorInfo FindConstructorMatchingContext(System.Type p_classType, Dictionary<string, object> p_context)
    {
        ConstructorInfo[] constructors = p_classType.GetConstructors ();

        ConstructorInfo ret = null;
        int maxParameters = 0;

        foreach (ConstructorInfo ci in constructors)
        {
            ParameterInfo[] parameters = ci.GetParameters ();

            int numberOfParameters = parameters.Length;

            if ((numberOfParameters >= maxParameters) &&
                (AllParametersMatch (parameters, p_context) == true))
            {
                maxParameters = numberOfParameters;
                ret = ci;
            }
        }

        return ret;
    }

    protected bool AllParametersMatch(ParameterInfo[] p_parameters, Dictionary<string, object> p_context)
    {
        foreach (ParameterInfo pi in p_parameters)
        {
            if (p_context.ContainsKey (pi.Name) == false)
            {
                UnityEngine.Debug.Log ("Parameter " + pi.Name + " not found in context");

                return false;
            }
        }

        return true;
    }

    protected string DictionaryKeysToString(Dictionary<string, object> p_context)
    {
        return string.Join (", ", new List<string> (p_context.Keys).ToArray ());
    }
}