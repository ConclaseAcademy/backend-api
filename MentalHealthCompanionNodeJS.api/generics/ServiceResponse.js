class ServiceResponse{
    constructor(status, message, data= null){
        this.status=status
        this.message=message
        this.data=data
    }
    static success(status= "success", message="successful", data=null){
        return new ServiceResponse(status, message,data)
    }
    static failure(status="failed", message="failed", data=null){
        return new ServiceResponse(status, message, data)
    }
}
module.exports= ServiceResponse;