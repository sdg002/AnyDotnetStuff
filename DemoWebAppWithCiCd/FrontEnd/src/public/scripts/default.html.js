import {WeatherEndpoint} from "./config.js"



export async function WeatherForeCastClickHandler(){
    
    let response=await fetch(WeatherEndpoint)
    console.log(`Got response ${response.status}`)
    let data=await response.json()
    console.log(`Got response ${data.length}`)
    document.getElementById("results").innerText=JSON.stringify(data)
}