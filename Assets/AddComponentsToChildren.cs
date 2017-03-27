using UnityEngine;
using System.Collections;
using System.Reflection;

// AddComponentsToChildren
//
// Very useful. Adds all components to all children at runtime awake

public class AddComponentsToChildren : MonoBehaviour
{
    public bool IsCopyProperties = false;                       // slow

    // interally populated
    private Component[] Templates;

    void Awake()
    {
        // Auto-Populate
        Templates = this.transform.GetComponents<Component>();

        // Check Templates
        if (Templates.Length > 0)
        {
            // Iterate Children
            for (int i = 0; i < this.transform.childCount; i++)
            {
                // Select Child
                Transform child = this.transform.GetChild(i);

                // Iterate Templates
                for (int t = 0; t < Templates.Length; t++)
                {
                    // Skip Transforms and AddComponentsToChildren Types
                    if (Templates[t] is Transform || Templates[t] is AddComponentsToChildren)
                        continue;

                    // Select Template
                    Component Template = Templates[t];

                    // Not Self And Doesn't Have This Template Yet
                    if (child != this.transform)
                    {
                        // Check If Doesn't Exist
                        if (child.GetComponent(Template.GetType()) == null)
                        {
                            // Add New Component
                            child.gameObject.AddComponent(Template.GetType());
                        }

                        // Get Component
                        var newComponent = child.GetComponent(Template.GetType());

                        // Copy Values From Template
                        if (newComponent != null)
                        {
                            // Special Case For Rigidbody Because Property Stuff Is Slow
                            if (newComponent is Rigidbody)
                            {
                                Rigidbody newRigid = (Rigidbody)newComponent;
                                newRigid.mass = ((Rigidbody)Template).mass;
                            }
                            else
                            {
                                // Copy Fields (they are fast)
                                FieldInfo[] templateFields = Template.GetType().GetFields();
                                FieldInfo[] newFields = newComponent.GetType().GetFields();

                                // Using Reflection
                                for (int f = 0; f < templateFields.Length; f++)
                                {
                                    newFields[f].SetValue(newComponent, templateFields[f].GetValue(Template));
                                }
                            }

                            // Copy properties (slow)
                            if (IsCopyProperties)
                            {
                                PropertyInfo[] templateProps = Template.GetType().GetProperties();
                                PropertyInfo[] newProps = newComponent.GetType().GetProperties();

                                // Using Reflection
                                for (int f = 0; f < templateProps.Length; f++)
                                {
                                    if (newProps[f].CanWrite
                                        && newProps[f].Name != "name"
                                        && newProps[f].Name != "tag"
                                        && newProps[f].Name != "position"
                                        && newProps[f].Name != "rotation")// && newProps[f].GetSetMethod(/*nonPublic*/ true).IsPublic)
                                    {
                                        Debug.Log(templateProps[f].Name);
                                        newProps[f].SetValue(newComponent, templateProps[f].GetValue(Template, null), null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void Start()
    {
        // Destroy Templates
        for (int i=0; i < Templates.Length; i++)
        {
            // Skip Transform
            if (Templates[i] is Transform)
                continue;
            Destroy(Templates[i]);
        }

        // Destroy Self
        Destroy(this);
    }
}
