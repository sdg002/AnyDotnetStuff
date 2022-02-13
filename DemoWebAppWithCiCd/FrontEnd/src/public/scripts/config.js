function GetEnvironmentFromUrl()
{
    let hostname=window.location.hostname;
    const envstrings=["dev","uat","prod"]
    let firstPathOfHost=hostname.split(".")[0]
    for(let i in envstrings){
        let envstring=envstrings[i];
        if (firstPathOfHost.toLowerCase().endsWith(envstring)){
            return envstring
        }
    }
    if (hostname === "localhost"){
        return "dev";
    }
    throw `could not parse the host ${hostname} name to get environment`
}

export const environment=GetEnvironmentFromUrl()
export const ProdServerUrl=`https://mydemowebapi123-${environment}.azurewebsites.net`
export const WeatherEndpoint=`${ProdServerUrl}/WeatherForecast`;