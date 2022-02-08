const { response } = require("express");

function ClickHandler(){
    //alert("click")
    fetch('data.txt')
    .then(response=>response.text())
    .then(text=>{
        console.log(`Fetched data ${text}`)
        document.getElementById("results").innerText=text    
    })
    .catch(error=>{
        //handle the error
        console.log(`There was an error ${error}`)
        console.log(error);
    });
    
}

function ClickHandlerWithFailure(){
    fetch('nonexistent.txt')
    .then(response=>{
        console.log(`Got response ${response.status}`)
    })
    .then(result => {
        console.log('Success:', result);
      })
    .catch(error=>{
        //handle the error
        console.log(`There was an error ${error}`)
        console.log(error);
    });

}

async function ClickHandlerWithAwait(){
    let response=await fetch('data.txt')
    console.log(`Got response ${response.status}`)
    let data=await response.text()
    document.getElementById("results").innerText=data
}