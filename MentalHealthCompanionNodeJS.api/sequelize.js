const {Sequelize}= require('sequelize');

require('dotenv').config() 
console.log (process.env.DB_DATABASE)
console.log(process.env.DB_USER)
console.log(process.env.DB_PASSWORD)
console.log(process.env.DB_HOST)
console.log("JWT_SECRET:", process.env.JWT_SECRET)

const sequelize= new Sequelize (
    process.env.DB_DATABASE,
    process.env.DB_USER,
    process.env.DB_PASSWORD,
    {
        host: process.env.DB_HOST,
        port: 5433,
        dialect: 'postgres'
    }
)

sequelize.authenticate() 
    .then(()=> console.log('connected to postgres via sequelize'))
    .catch((err)=> console.error('error occured while connecting to postgres. Damn', err))

    sequelize.sync({ force: false })
    .then(() => console.log('Models synced'))
    .catch((err) => console.error('Sync error:', err));
  
    module.exports= sequelize; 