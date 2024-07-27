using System;
using EnvDTE;
using EnvDTE80;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Linq;
using System.Security.AccessControl;

namespace BusinessLayer.CreateorBusiness
{
    public enum enReferenceType { ItemGroup, NugetPackage, None }
    public static class SolutionCreateor
    {
        private const string VisualStudioProgID = "VisualStudio.DTE.17.0";

        static private DTE2 GetDTEInstance()
            => (DTE2)Activator.CreateInstance(Type.GetTypeFromProgID(VisualStudioProgID));

        static private void ReleaseDTEInstance(DTE2 VSObj)
        {
            if (VSObj != null)
            {
                VSObj.Quit();
                Marshal.FinalReleaseComObject(VSObj);
                VSObj = null;
            }
        }

        static private string GetSolutionPath(string solutionPath)
        {
            if (string.IsNullOrEmpty(solutionPath))
                throw new ArgumentNullException("solutionPath was null");
            int dotpos = solutionPath.LastIndexOf(".");
            if (dotpos == -1)
                throw new Exception("Path not correct");
            return solutionPath.Remove(dotpos, solutionPath.Length - dotpos);
        }

        static public void CreateSolution(string Dir, string Name)
        {
            DTE2 VSObj = GetDTEInstance();
            try
            {
                VSObj.Solution.Create(Dir, Name);
                VSObj.Solution.SaveAs(Path.Combine(Dir, Name + ".sln"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (VSObj != null)
                {
                    ReleaseDTEInstance(VSObj);
                }
            }
        }
        
        static public void CreateProject(string SolutionPath, string Name)
        {
            DTE2 VSObj = GetDTEInstance();
            try
            {
                VSObj.Solution.Open(Path.GetFullPath(SolutionPath));

                string TemplatePath = ((Solution2)VSObj.Solution).GetProjectTemplate("Classlibrary.zip", "CSharp");

                if (string.IsNullOrEmpty(TemplatePath))
                    throw new InvalidOperationException("Template Path doesn't found");


                string ProjectPath = Path.Combine(GetSolutionPath(SolutionPath), Name);
                VSObj.Solution.AddFromTemplate(TemplatePath, ProjectPath, Name);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (VSObj != null)
                {
                    ReleaseDTEInstance(VSObj);
                }
            }
        }
        
        static public void CreateFiles(string SolutionPath, string ProjectName, string FileName, string Content)
        {
            DTE2 VSObj = GetDTEInstance();
            try
            {
                // Open The Solution
                VSObj.Solution.Open(SolutionPath);
                Project Project = null;

                // Search for the project in solution 
                foreach (Project Proj in VSObj.Solution.Projects)
                    if (Proj.Name == ProjectName)
                        Project = Proj;

                // If Project is null that mean the project name doesn't found
                if (Project == null)
                    throw new InvalidOperationException($"Project with {ProjectName} not found");

                // Create File Path 
                string FilePath = Path.Combine(GetSolutionPath(SolutionPath), ProjectName, FileName + ".cs");

                // Write The Content of file in file
                File.WriteAllText(FilePath, Content);
                // Ad File To ProjectsItems
                Project.ProjectItems.AddFromFile(FilePath);
                // Save Changes in Solution
                VSObj.Solution.SaveAs(SolutionPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ReleaseDTEInstance(VSObj);
            }
        }

        static private void AddNugetReferenceToProjectFile(string projectFilePath, string reference)
        {
            // Cinvert The .csproj file to XML File
            XDocument projectFile = XDocument.Load(projectFilePath);
            // Access to Element
            XElement projectElement = projectFile.Element("Project");

            // Ensure that there is an ItemGroup for references
            XElement itemGroup = new XElement("ItemGroup");
            XElement referenceElement = new XElement("PackageReference",
                new XAttribute("Include", reference), new XAttribute("Version", "5.2.1"));
            itemGroup.Add(referenceElement);

            // Add the ItemGroup to the project file
            projectElement.Add(itemGroup);
            projectFile.Save(projectFilePath);
        }

        static private void AddReferenceToProjectFile(string projectFilePath, string reference)
        {
            // Cinvert The .csproj file to XML File
            XDocument projectFile = XDocument.Load(projectFilePath);
            // Access to Element
            XElement projectElement = projectFile.Element("Project");

            // Ensure that there is an ItemGroup for references
            XElement itemGroup = new XElement("ItemGroup");
            XElement referenceElement = new XElement("ProjectReference",
                new XAttribute("Include", reference));
            itemGroup.Add(referenceElement);

            // Add the ItemGroup to the project file
            projectElement.Add(itemGroup);
            projectFile.Save(projectFilePath);
        }

        static public void Addreference(string projectFilePath,enReferenceType RefType, string reference)
        {
            if (RefType == enReferenceType.ItemGroup)
                AddReferenceToProjectFile(projectFilePath, reference);
            else
                AddNugetReferenceToProjectFile(projectFilePath, reference); 
        }
    }
}
