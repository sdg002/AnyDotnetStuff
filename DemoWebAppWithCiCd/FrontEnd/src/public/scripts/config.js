function GetEnvironmentFromUrl()
{
    let hostname=window.location.hostname;
    if (hostname === "localhost"){
        return "dev";
    }
    let frags=hostname.split("-");
    if (frags.length <2){
        throw `could not parse the host ${hostname} name to get environment`
    }
    return frags[1];
}

export const environment=GetEnvironmentFromUrl()
export const ProdServerUrl=`https://mydemowebapi123-${environment}.azurewebsites.net`
export const WeatherEndpoint=`${ProdServerUrl}/WeatherForecast`;