const bcrypt= require("bcrypt")
const {DataTypes}= require('sequelize')
const sequelize= require ('../sequelize')

const User= sequelize.define('users', {
    id: {
        type: DataTypes.UUID,
        defaultValue: DataTypes.UUIDV4,
        primaryKey: true,

    },
    email:{
        type:DataTypes.STRING,
        allowNull: false,
        unique: true,

    },
    password:{
        type:DataTypes.STRING,
        allowNull: false,

    }
    

},
{
    timestamps: true,
    hooks:{
        beforeCreate: async (user)=>{
            user.password= await bcrypt.hash(user.password,10)

        }
    }
}
)
sequelize.sync();
module.exports= User;