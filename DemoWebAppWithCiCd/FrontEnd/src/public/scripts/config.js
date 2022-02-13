function GetEnvironmentFromUrl()
{
    let hostname=window.location.hostname;
    const envstrings=["dev","uat","prod"]
    for(let i in envstrings){
        let envstring=envstrings[i];
        if (hostname.toLowerCase().endsWith(envstring)){
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