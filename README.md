# AnalysisServices.Deployment
Wrapper around the default Analysis Service Deployment wizard of Microsoft.

Adding aditional options, like
- Provide different input files
- Generate impersonation security information

Syntax:
/d             Deployment mode
/m:[filename]  Model definition
/t:[filename]  Deployment target
/o:[filename]  Deployment options
/c:[filename]  Deployment config
/s:[filename]  Deployment security
/a:[filename]  Deployment assembly

/i             Impersonation mode
/ds:[ID]       Datasource ID
/u:[username]  Impersonation username
/p:[password]  Impersonation password
/f:[filename]  Export filename

/?, /h         This help
