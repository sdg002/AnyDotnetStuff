import {WeatherEndpoint} from "./config.js"



export async function FetchWeatherForecastHandler(tableElement){
    try{
        ClearWeatherForecastHandler(tableElement)
        console.log("fetching weather")
        let response=await fetch(WeatherEndpoint)
        console.log(`Got response ${response.status}`)
        let data=await response.json()
        console.log(`Got response ${data.length}`)
    
        let tbodyElement=tableElement.querySelector("tbody")
        for (let i in data) {
            let weather=data[i]
            let tr=document.createElement("tr")
    
            let date=CreateTHElement(weather.date)
            tr.append(date)
    
            let tempF=CreateTHElement(weather.temperatureF)
            tr.append(tempF)
    
            let tempC=CreateTHElement(weather.temperatureC)
            tr.append(tempC)
    
            let summary=CreateTHElement(weather.summary)
            tr.append(summary)
            tbodyElement.append(tr)
          }    
    
    }
    catch(e){
        alert(e);
    }
}

function CreateTHElement(text){
    let th=document.createElement("td")
    let textElement=document.createTextNode(text)
    th.append(textElement)
    return th
}

export function ClearWeatherForecastHandler(tableElement){
    let tbodyElement=tableElement.querySelector("tbody")
    console.log("clear weather")
    while (tbodyElement.hasChildNodes()) {
        tbodyElement.removeChild(tbodyElement.childNodes[0]);
      }
}
