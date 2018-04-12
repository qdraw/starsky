import React, { Component } from 'react';
import { TouchableOpacity } from 'react-native';
import { StackNavigator} from 'react-navigation'
import HomeScreen from './HomeScreen';


const RootStack = StackNavigator(
    {
      Home: {
        screen: HomeScreen,
      }
    },
    {
      initialRouteName: 'Home',
      navigationOptions: {
        headerStyle: {
          backgroundColor: '#f4511e',
        },
        headerTintColor: '#fff',
        headerTitleStyle: {
          fontWeight: 'bold',
        },
      },
    }
  );

export default RootStack;