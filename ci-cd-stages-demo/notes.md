[[_TOC_]]

# Objective
Azure Devops in 2023 has gained wide popularity in the developer community. Azure Devops is a completel suite of tools to manage your software development lifecyle.  The CI/CD is one of the major components of Azure Devops. You could be using Azure Devops for the product management , but you could be using some other tool for CI/CD (e.g. Jenkins and Octopus).  YAML based pipelines of Azure Devops is a powerful CI/CD orchestration tool. However, it would be an understatement to state that this is an easy tool. I have found that authoring Azure Devops CI/CD can be time consuming and hard to troubleshoot at times. The lesson that I have learnt - keep the CI/CD simple and linear!  (I will explain what I mean by linear)


---

# Source code and pipeline

## Link to Repo
https://github.com/sdg002/AnyDotnetStuff/activity?ref=master


## Link to Devops pipeline
https://dev.azure.com/docxreview/devops001/_build?definitionId=8

---
# Step 100-Simple skeletal CI/CD YAML with 1 BUILD and 1 DEV_DEPLOY stages

What are we showing here ?

- 1 master YAML
- Split into stages (BUILD and DEV_DEPLOY)
- Using Build and Deploy templates
- Specify the name=major.minor.path.build.buildid


[[SHOW A PICTURE OF THE YAML, BUILD AND RELEASE TEMPLATE]]

![docs/ppt-images/](docs/ppt-images/cicd.png)

---

# References and articles

## Templates usage reference
https://learn.microsoft.com/en-us/azure/devops/pipelines/process/templates?view=azure-devops&pivots=templates-includes

## How to use parameters ?
https://damienaicheh.github.io/azure/devops/2021/02/10/variable-templates-azure-devops-en.html

## Publish and download pipeline Artifacts
https://learn.microsoft.com/en-us/azure/devops/pipelines/artifacts/pipeline-artifacts?view=azure-devops&tabs=yaml

---
