const passport= require("../passportJwtValidation")
const express =require("express")
const router= express.Router()

const authService = require("../services/auth.service");
const ServiceResponse = require("../generics/ServiceResponse");

router.post("/create-user", async (req,res)=> {
    var response= await authService.createUser(req,res) 
    res.send(response)
})
router.post("/login", async (req,res)=>{
    console.log(req.body)
    var response = await authService.login(req,res)
    res.send(response)
})

router.get("/profile", passport.authenticate, async (req,res)=>{
    res.json(ServiceResponse.success("success", "profile retrived successfully", req.user ))
})
module.exports=router;